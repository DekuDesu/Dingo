using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using DingoAuthentication.Encryption;

namespace DingoAuthentication.Tests
{
    public class EncryptionClientTests
    {
        public static TmpLogger<DiffieHellmanRatchet> logger = new();
        public static TmpLogger<DiffieHellmanHandler> dhLogger = new();
        public static TmpLogger<SignatureHandler> sLogger = new();
        public static TmpLogger<SymmetricHandler<EncryptedDataModel>> smLogger = new();
        public static TmpLogger<EncryptionClient<EncryptedDataModel, KeyBundleModel, SignedKeyModel>> cllogger = new();
        public static TmpLogger<KeyDerivationRatchet<EncryptedDataModel>> kdfLogger = new();

        private static Random Generator = new();

        [Fact]
        public void EncryptionClientBundleGeneratesCorrectly()
        {
            var client = CreateClient();

            var bundle = client.GenerateBundle();

            // identity key should be 91 bytes
            Assert.True(bundle.X509IdentityKey.Length == 91);

            // public key should be 72 bytes
            Assert.True(bundle.PublicKey.PublicKey.Length == 72);

            // signature should be 64 bytes
            Assert.True(bundle.PublicKey.Signature.Length == 64);

            Assert.True(DHRatchetTests.VerifyBytes(bundle.X509IdentityKey, client.dhRatchet.X509IdentityKey));

            Assert.True(DHRatchetTests.VerifyBytes(bundle.PublicKey.PublicKey, client.dhRatchet.PublicKey));

            Assert.True(DHRatchetTests.VerifyBytes(bundle.PublicKey.Signature, client.dhRatchet.IdentitySignature));
        }

        [Fact]
        public void AbleToCreateSecrets()
        {
            var alice = CreateClient();

            var alicesBundle = alice.GenerateBundle();

            var bob = CreateClient();

            var bobsBundle = bob.GenerateBundle();

            // make sure the functions return correctly
            Assert.True(alice.CreateSecretUsingBundle(bobsBundle));

            Assert.True(bob.CreateSecretUsingBundle(alicesBundle));

            // make sure a secret was actually created using those budles,
            // just becuase the functions returned true does not necissarily mean that 
            // a secret was successfully created - unless we test it that is
            Assert.True(VerifyBytes(alice.dhRatchet.PrivateKey, bob.dhRatchet.PrivateKey));
        }

        [Fact]
        public void StateCanBeExportedAndImported()
        {
            var alice = CreateClient();

            var alicesBundle = alice.GenerateBundle();

            var bob = CreateClient();

            var bobsBundle = bob.GenerateBundle();

            // make sure the functions return correctly
            Assert.True(alice.CreateSecretUsingBundle(bobsBundle));

            Assert.True(bob.CreateSecretUsingBundle(alicesBundle));

            // make sure a secret was actually created using those budles,
            // just becuase the functions returned true does not necissarily mean that 
            // a secret was successfully created - unless we test it that is
            Assert.True(VerifyBytes(alice.dhRatchet.PrivateKey, bob.dhRatchet.PrivateKey));

            string data = "Hello World";

            Assert.True(alice.TryEncrypt(data, out EncryptedDataModel encryptedData));

            Assert.True(bob.TryDecrypt(encryptedData, out string result));

            Assert.Equal(data, result);

            string aliceState = alice.ExportState();

            alice = CreateClient();

            alice.ImportState(aliceState);

            Assert.True(VerifyBytes(alice.dhRatchet.PrivateKey, bob.dhRatchet.PrivateKey));

            data = "Hello World 2";

            Assert.True(alice.TryEncrypt(data, out encryptedData));

            Assert.True(bob.TryDecrypt(encryptedData, out result));

            Assert.Equal(data, result);
        }

        [Fact]
        public void RatchetWorks()
        {
            // create secret
            var alice = CreateClient();

            var alicesBundle = alice.GenerateBundle();

            var bob = CreateClient();

            var bobsBundle = bob.GenerateBundle();

            // make sure the functions return correctly
            Assert.True(alice.CreateSecretUsingBundle(bobsBundle));

            Assert.True(bob.CreateSecretUsingBundle(alicesBundle));

            // make sure a secret was actually created using those budles,
            // just becuase the functions returned true does not necissarily mean that 
            // a secret was successfully created - unless we test it that is
            Assert.True(VerifyBytes(alice.dhRatchet.PrivateKey, bob.dhRatchet.PrivateKey));

            // now that we have a secret we should make sure after n ratchets both parties have the same keys

            for (int i = 0; i < Generator.Next(0, 1000); i++)
            {
                alice.RatchetDiffieHellman();
                bob.RatchetDiffieHellman();
            }

            Assert.True(VerifyBytes(alice.dhRatchet.PrivateKey, bob.dhRatchet.PrivateKey));
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

        EncryptionClient<EncryptedDataModel, KeyBundleModel, SignedKeyModel> CreateClient()
        {
            return new EncryptionClient<EncryptedDataModel, KeyBundleModel, SignedKeyModel>(cllogger, CreateRatchet(), CreateKDFRatchet(), CreateKDFRatchet());
        }

        private IDiffieHellmanRatchet CreateRatchet()
        {
            return new DiffieHellmanRatchet(logger, new DiffieHellmanHandler(dhLogger), new KeyDerivationFunction(), new SignatureHandler(sLogger));

        }

        private IKeyDerivationRatchet<EncryptedDataModel> CreateKDFRatchet()
        {
            return new KeyDerivationRatchet<EncryptedDataModel>(kdfLogger, new KeyDerivationFunction(), new SymmetricHandler<EncryptedDataModel>(smLogger));
        }
    }
}
