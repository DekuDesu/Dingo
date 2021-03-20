using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DingoAuthentication.Encryption;
using Xunit;

namespace DingoAuthentication.Tests
{
    public class KDFTests
    {
        public static TmpLogger<DiffieHellmanRatchet> logger = new();
        public static TmpLogger<DiffieHellmanHandler> dhLogger = new();
        public static TmpLogger<SignatureHandler> sLogger = new();
        private static TmpLogger<KeyDerivationRatchet<EncryptedDataModel>> kdrLogger = new();
        public static TmpLogger<SymmetricHandler<EncryptedDataModel>> smLogger = new();

        private static SymmetricHandler<EncryptedDataModel> aes = new(smLogger);

        [Fact]
        public void KeySizeMatches()
        {
            var dh = CreateDiffieRatchet();
            var ratchet = CreateRatchet();

            // seed the ratchet
            ratchet.Reset(dh.PrivateKey);

            byte[] key;

            Assert.True(ratchet.GenerateNextKey(out key));

            // public keys are 32 bytes and public keys are 72 bytes
            Assert.True(key.Length == 32);
        }

        [Theory]
        [InlineData("Stata")]
        [InlineData("null")]
        [InlineData(" ")]
        [InlineData("*")]
        [InlineData("DROP TABLE ;")]
        [InlineData("01 22 001 22002 29929")]
        public void KeysGeneratedCanBeUsedToEncrypt(string dataToEncrypt)
        {
            var dh = CreateDiffieRatchet();
            var alice = CreateRatchet();
            var bob = CreateRatchet();

            // seed the ratchets
            alice.Reset(dh.PrivateKey);
            bob.Reset(dh.PrivateKey);

            byte[] PrivateKey;

            Assert.True(alice.GenerateNextKey(out PrivateKey));

            EncryptedDataModel data;

            Assert.True(aes.TryEncrypt(dataToEncrypt, PrivateKey, out data));

            byte[] BobsPrivateKey;
            Assert.True(bob.GenerateNextKey(out BobsPrivateKey));

            string result;

            Assert.True(aes.TryDecrypt(data, BobsPrivateKey, out result));

            Assert.Equal(dataToEncrypt, result);

            Assert.True(alice.GenerateNextKey(out PrivateKey));

            Assert.True(aes.TryEncrypt(dataToEncrypt, PrivateKey, out data));

            Assert.True(bob.GenerateNextKey(out BobsPrivateKey));

            Assert.True(aes.TryDecrypt(data, BobsPrivateKey, out result));

            Assert.Equal(dataToEncrypt, result);
        }

        private IKeyDerivationRatchet<EncryptedDataModel> CreateRatchet()
        {
            return new KeyDerivationRatchet<EncryptedDataModel>(kdrLogger, new KeyDerivationFunction(), new SymmetricHandler<EncryptedDataModel>(smLogger));
        }
        private IDiffieHellmanRatchet CreateDiffieRatchet()
        {
            return new DiffieHellmanRatchet(logger, new DiffieHellmanHandler(dhLogger), new KeyDerivationFunction(), new SignatureHandler(sLogger));

        }
    }
}
