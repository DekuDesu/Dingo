using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using DingoAuthentication.Encryption;
using System.Security.Cryptography;
using Newtonsoft.Json;

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

            Assert.NotNull(ratchet.X509IdentityKey);
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

            Assert.True(signer.Verify(ratchet.PublicKey, ratchet.IdentitySignature, ratchet.X509IdentityKey));
        }

        [Fact]
        public void SuccessfullyCreateSharedSecret()
        {

            IDiffieHellmanRatchet alice = CreateRatchet();
            IDiffieHellmanRatchet bob = CreateRatchet();

            // make sure they're not the same person that would be awkward
            Assert.True(alice.X509IdentityKey != bob.X509IdentityKey);

            Assert.True(bob.TryCreateSharedSecret(alice.X509IdentityKey, alice.PublicKey, alice.IdentitySignature));

            Assert.True(alice.TryCreateSharedSecret(bob.X509IdentityKey, bob.PublicKey, bob.IdentitySignature));

            // now that we have created a shared secret make sure theyre the same secret
            Assert.True(VerifyBytes(alice.PrivateKey, bob.PrivateKey));
        }

        [Fact]
        public void EnsureIncorrectSignatureCantCreateSecret()
        {
            IDiffieHellmanRatchet alice = CreateRatchet();
            IDiffieHellmanRatchet bob = CreateRatchet();

            // make sure they're not the same person that would be awkward
            Assert.True(alice.X509IdentityKey != bob.X509IdentityKey);

            Assert.True(bob.TryCreateSharedSecret(alice.X509IdentityKey, alice.PublicKey, alice.IdentitySignature));

            byte[] sig = bob.IdentitySignature;

            Random r = new();

            sig = sig.Select(x => (byte)r.Next(byte.MinValue, byte.MaxValue)).ToArray();

            Assert.False(alice.TryCreateSharedSecret(bob.X509IdentityKey, bob.PublicKey, sig));
        }

        [Fact]
        public void RatchetProducesKeysThatCanBeUsedToEncrypt()
        {
            IDiffieHellmanRatchet alice = CreateRatchet();
            IDiffieHellmanRatchet bob = CreateRatchet();

            // make sure they're not the same person that would be awkward
            Assert.True(alice.X509IdentityKey != bob.X509IdentityKey);

            if (alice is DiffieHellmanRatchet df)
            {
                // "\"MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEyLRqUNQMLQDPzMPA6EDpmCt4FJ41/z6B36IoyP2YZDDwshdm+79Cq+FacJskIWFItpaTXhyi0PsMO0E09EioVA==\""
                var x = JsonConvert.SerializeObject(df.X509IdentityKey);
                // "\"RUNLMSAAAACNHkLrN+uQ9nPbmcA1ycltM5nDhT+WXjfYxqroZDzADnWrj0cs+9dy9RQ8zRASehjdCsGXxvFTsleL8HlgKUXE\""
                var y = JsonConvert.SerializeObject(df.PublicKey);
                // "\"h5XA+r0d2cPwDjhn6ABs8/OEo8mmhbkspqHCFCPNzM4NNsPOp8wpQP0N0TbR0Z0t0NiUS/cgxrem3krrOgZ3nA==\""
                var z = JsonConvert.SerializeObject(df.IdentitySignature);
                _ = x;
                _ = y;
                _ = z;
                /*
                 {
                    "x509IdentityKey": "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEyLRqUNQMLQDPzMPA6EDpmCt4FJ41/z6B36IoyP2YZDDwshdm+79Cq+FacJskIWFItpaTXhyi0PsMO0E09EioVA==",
                    "signature": "h5XA+r0d2cPwDjhn6ABs8/OEo8mmhbkspqHCFCPNzM4NNsPOp8wpQP0N0TbR0Z0t0NiUS/cgxrem3krrOgZ3nA==",
                    "publicKey": "RUNLMSAAAACNHkLrN+uQ9nPbmcA1ycltM5nDhT+WXjfYxqroZDzADnWrj0cs+9dy9RQ8zRASehjdCsGXxvFTsleL8HlgKUXE",
                    "id": "string"
                }
                 */
            }

            Assert.True(bob.TryCreateSharedSecret(alice.X509IdentityKey, alice.PublicKey, alice.IdentitySignature));

            Assert.True(alice.TryCreateSharedSecret(bob.X509IdentityKey, bob.PublicKey, bob.IdentitySignature));



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

            Assert.True(aes.TryEncrypt("Hello World", alice.PrivateKey, out encryptedData));

            Assert.True(aes.TryDecrypt(encryptedData, bob.PrivateKey, out decryptedData));

            Assert.Equal("Hello World", decryptedData);

            Assert.True(aes.TryEncrypt("Hello World", alice.PrivateKey, out encryptedData));

            Assert.True(aes.TryDecrypt(encryptedData, bob.PrivateKey, out decryptedData));

            Assert.Equal("Hello World", decryptedData);
        }

        [Fact]
        public void GeneratingNewKeysSignsCorrectly()
        {
            // we want to make sure we we generate new keys they are being signed immediatelyt and correctly
            // we want to make sure the signature for the identity key matches
            IDiffieHellmanRatchet ratchet = CreateRatchet();

            ISignatureHandler signer = new SignatureHandler(sLogger);

            Assert.True(signer.Verify(ratchet.PublicKey, ratchet.IdentitySignature, ratchet.X509IdentityKey));

            byte[] oldKey = ratchet.PrivateKey;

            ratchet.GenerateBaseKeys();

            // make sure the new keys verify correctly
            Assert.True(signer.Verify(ratchet.PublicKey, ratchet.IdentitySignature, ratchet.X509IdentityKey));

            // make sure the new key isn't the old key
            Assert.False(VerifyBytes(oldKey, ratchet.PrivateKey));
        }


        [Theory]
        [InlineData(1_000)]
        public void GeneratingKeysNeverRepeat(int numberOfKeys)
        {
            IDiffieHellmanRatchet alice = CreateRatchet();
            IDiffieHellmanRatchet bob = CreateRatchet();

            // make sure they're not the same person that would be awkward
            Assert.True(alice.X509IdentityKey != bob.X509IdentityKey);

            // make sure they sucessfully create a secret
            Assert.True(bob.TryCreateSharedSecret(alice.X509IdentityKey, alice.PublicKey, alice.IdentitySignature));

            Assert.True(alice.TryCreateSharedSecret(bob.X509IdentityKey, bob.PublicKey, bob.IdentitySignature));

            HashSet<string> hashedKeys = new HashSet<string>();

            for (int i = 0; i < numberOfKeys; i++)
            {
                if (bob.TryRatchet(out byte[] newKey))
                {
                    Assert.True(hashedKeys.Add(Hash(newKey)));

                    // private keys generated should always be 32
                    Assert.True(newKey.Length == 32);
                }
            }

            Assert.True(hashedKeys.Count == numberOfKeys);

            // make sure alice generates all of the same keys

            for (int i = 0; i < numberOfKeys; i++)
            {
                if (alice.TryRatchet(out byte[] newKey))
                {
                    Assert.False(hashedKeys.Add(Hash(newKey)));

                    // private keys generated should always be 32
                    Assert.True(newKey.Length == 32);
                }
            }
        }

        private string Hash(byte[] bytes)
        {
            byte[] hashed = SHA256.Create().ComputeHash(bytes);
            return BitConverter.ToString(hashed);
        }

        public static bool VerifyBytes(IEnumerable<byte> first, IEnumerable<byte> second)
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
