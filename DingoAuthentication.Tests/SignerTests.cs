using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using DingoAuthentication.Encryption;
using System.Security.Cryptography;

namespace DingoAuthentication.Tests
{
    public class SignerTests
    {
        public static TmpLogger<SignatureHandler> logger = new TmpLogger<SignatureHandler>();
        public static Random Generator = new();
        [Fact]
        public void SignerReturnsWithInvalid()
        {
            SignatureHandler handler = new SignatureHandler(logger);

            // make sure it prevents null or empty inputs

            byte[] input = null;

            Assert.False(handler.TrySign(input, NewKey(), out _, out _));

            input = new byte[] { };

            Assert.False(handler.TrySign(input, NewKey(), out _, out _));

            input = new byte[] { 1, 2, 3, 4, 5, 6 };

            // make sure it prevents null or empty keys

            byte[] key = null;

            Assert.False(handler.TrySign(input, key, out _, out _));

            key = new byte[] { };

            Assert.False(handler.TrySign(input, key, out _, out _));

        }

        [Fact]
        public void SignerDoesntReturnFalseWithValid()
        {
            SignatureHandler handler = new SignatureHandler(logger);

            // make sure it prevents null or empty inputs

            byte[] input = new byte[] { 1, 2, 3, 4, 5 };

            Assert.True(handler.TrySign(input, NewKey(), out _, out _));
        }

        [Fact]
        public void SignerActuallySigns()
        {
            SignatureHandler handler = new SignatureHandler(logger);

            // make sure it prevents null or empty inputs

            byte[] input = NewKey();

            byte[] publicKey;

            Assert.True(handler.TrySign(input, input, out var signature, out publicKey));

            Assert.True(handler.Verify(input, signature, publicKey));
        }

        private byte[] NewKey()
        {
            var dh = new DiffieHellmanHandler(null);

            return dh.GenerateKeys().PrivateKey;
        }
    }
}
