using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using DingoAuthentication.Encryption;

namespace DingoAuthentication.Tests
{
    public class DHRatchetTests
    {
        public static TmpLogger<DiffieHellmanRatchet> logger = new();
        public static TmpLogger<DiffieHellmanHandler> dhLogger = new();
        public static TmpLogger<SignatureHandler> sLogger = new();
        public static TmpLogger<SymmetricHandler<EncryptedDataModel>> smLogger = new();

        [Fact]
        public void ConstructionWorks()
        {
            IDiffieHellmanRatchet ratchet = CreateRatchet();

            Assert.NotNull(ratchet.X509IndentityKey);
            Assert.NotNull(ratchet.PublicKey);
            Assert.NotNull(ratchet.PrivateKey);
            Assert.NotNull(ratchet.IdentitySignature);
        }

        [Fact]
        public void SignatureVerifies()
        {
            // we want to make sure the signature for the identity key matches
            IDiffieHellmanRatchet ratchet = CreateRatchet();

            ISignatureHandler signer = new SignatureHandler(sLogger);

            Assert.True(signer.Verify(ratchet.PublicKey, ratchet.IdentitySignature, ratchet.X509IndentityKey));
        }

        [Fact]
        public void SuccessfullyCreateSharedSecret()
        {
            IDiffieHellmanRatchet alice = CreateRatchet();
            IDiffieHellmanRatchet bob = CreateRatchet();

            // make sure they're not the same person that would be awkward
            Assert.True(alice.X509IndentityKey != bob.X509IndentityKey);

            Assert.True(bob.TryCreateSharedSecret(alice.X509IndentityKey, alice.PublicKey, alice.IdentitySignature));

            Assert.True(alice.TryCreateSharedSecret(bob.X509IndentityKey, bob.PublicKey, bob.IdentitySignature));

            // now that we have created a shared secret make sure theyre the same secret
            Assert.True(VerifyBytes(alice.PrivateKey, bob.PrivateKey));
        }

        [Fact]
        public void EnsureIncorrectSignatureCantCreateSecret()
        {
            IDiffieHellmanRatchet alice = CreateRatchet();
            IDiffieHellmanRatchet bob = CreateRatchet();

            // make sure they're not the same person that would be awkward
            Assert.True(alice.X509IndentityKey != bob.X509IndentityKey);

            Assert.True(bob.TryCreateSharedSecret(alice.X509IndentityKey, alice.PublicKey, alice.IdentitySignature));

            byte[] sig = bob.IdentitySignature;

            Random r = new();

            sig = sig.Select(x => (byte)r.Next(byte.MinValue, byte.MaxValue)).ToArray();

            Assert.False(alice.TryCreateSharedSecret(bob.X509IndentityKey, bob.PublicKey, sig));
        }

        [Fact]
        public void RatchetProducesKeysThatCanBeUsedToEncrypt()
        {
            IDiffieHellmanRatchet alice = CreateRatchet();
            IDiffieHellmanRatchet bob = CreateRatchet();

            // make sure they're not the same person that would be awkward
            Assert.True(alice.X509IndentityKey != bob.X509IndentityKey);

            Assert.True(bob.TryCreateSharedSecret(alice.X509IndentityKey, alice.PublicKey, alice.IdentitySignature));

            Assert.True(alice.TryCreateSharedSecret(bob.X509IndentityKey, bob.PublicKey, bob.IdentitySignature));

            // store this for later testing
            byte[] oldSharedSecret = alice.PrivateKey;

            // now that we have created a shared secret make sure theyre the same secret
            Assert.True(VerifyBytes(alice.PrivateKey, bob.PrivateKey));

            // make sure the keys that are produced actually work
            SymmetricHandler<EncryptedDataModel> aes = new(smLogger);

            EncryptedDataModel encryptedData;

            Assert.True(aes.TryEncrypt("Hello World", alice.PrivateKey, out encryptedData));

            // make sure it can also be decrypted
            string decryptedData;
            Assert.True(aes.TryDecrypt(encryptedData, bob.PrivateKey, out decryptedData));

            Assert.Equal("Hello World", decryptedData);

            // make sure the ratchet works

            Assert.True(alice.TryRatchet(out _));

            Assert.True(bob.TryRatchet(out _));

            // make sure the new values are the same for both people
            Assert.True(VerifyBytes(alice.PrivateKey, bob.PrivateKey));

            // make sure the new key isnt the same key as before that would be disasterous
            Assert.False(VerifyBytes(alice.PrivateKey, oldSharedSecret));
        }

        [Fact]
        public void GeneratingNewKeysSignsCorrectly()
        {
            // we want to make sure we we generate new keys they are being signed immediatelyt and correctly
            // we want to make sure the signature for the identity key matches
            IDiffieHellmanRatchet ratchet = CreateRatchet();

            ISignatureHandler signer = new SignatureHandler(sLogger);

            Assert.True(signer.Verify(ratchet.PublicKey, ratchet.IdentitySignature, ratchet.X509IndentityKey));

            byte[] oldKey = ratchet.PrivateKey;

            ratchet.GenerateBaseKeys();

            // make sure the new keys verify correctly
            Assert.True(signer.Verify(ratchet.PublicKey, ratchet.IdentitySignature, ratchet.X509IndentityKey));

            // make sure the new key isn't the old key
            Assert.False(VerifyBytes(oldKey, ratchet.PrivateKey));
        }

        private bool VerifyBytes(IEnumerable<byte> first, IEnumerable<byte> second)
        {
            if (first is null ^ second is null)
            {
                return false;
            }
            if (first.Count() != second.Count())
            {
                return false;
            }

            for (int i = 0; i < first.Count(); i++)
            {
                if (first.ElementAt(i) != second.ElementAt(i))
                {
                    return false;
                }
            }
            return true;
        }

        private IDiffieHellmanRatchet CreateRatchet()
        {
            return new DiffieHellmanRatchet(logger, new DiffieHellmanHandler(dhLogger), new KeyDerivationFunction(), new SignatureHandler(sLogger));

        }
    }
}
