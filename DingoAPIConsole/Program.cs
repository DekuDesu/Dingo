using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DingoAuthentication;
using DingoAuthentication.Tests;
using DingoDataAccess.Compression;
using Microsoft.Extensions.Logging;
using DingoAuthentication.Encryption;
using System.Net.Http;

namespace DingoAuthentication.Tests
{
    public class program
    {
        private static Encoding Encoder = new UnicodeEncoding();

        private static HttpClient client = new();

        private static ISymmetricHandler<EncryptedDataModel> symmetricHandler = new SymmetricHandler<EncryptedDataModel>(smLogger);
        private static ISignatureHandler signatureHandler = new SignatureHandler(new TmpLogger<SignatureHandler>());
        private static string TestingAccountId = "f80a8358-3e27-4daa-9389-9c9462b6620c";
        private static string OAuthTestingKey = "041118e3-fb19-4c19-ae4d-c97e5fc1b437";

        public static async Task Main()
        {
            Console.OutputEncoding = Encoding.Unicode;

            await Wait(100);


            Console.WriteLine("Press enter to run test");

            Console.ReadLine();

            await Simulate_MassSessionRequest(1);

            //for (int i = 0; i < 2; i++)
            //{
            //    _ = Task.Run(() => Simulate_MassSessionRequest(200));
            //}

            Console.ReadLine();
            Console.ReadLine();
        }

        private class ServerKey
        {
            public byte[] X509IdentityKey { get; set; }
        }

        private class HandshakeObject
        {
            public string Id { get; set; }
            public byte[] X509IdentityKey { get; set; }
            public byte[] PublicKey { get; set; }
            public byte[] Signature { get; set; }
        }

        private class EncryptedMessageModel
        {
            public string Id { get; set; }
            public IEncryptedDataModel EncryptedData { get; set; }
        }

        private class AuthenticationRequestModel
        {
            public string Id { get; set; }
            public string OAuth { get; set; }
        }

        private static async Task Simulate_MassSessionRequest(int sessions = 10)
        {
            Console.WriteLine($"Simulating {sessions} hitting /API_Sessions");

            for (int i = 0; i < sessions; i++)
            {
                await RequestSession();
            }

            Console.WriteLine($"Finished Simulating Sessions");
        }

        private static async Task RequestSession()
        {
            var ratchet = CreateRatchet();

            ratchet.GenerateBaseKeys();

            Console.WriteLine("Getting server's public key");
            Console.WriteLine("/EncryptedSessions GET");

            ServerKey response = Newtonsoft.Json.JsonConvert.DeserializeObject<ServerKey>(await client.GetStringAsync("https://localhost:5001/API_Sessions"));

            Console.WriteLine($"Server X509IdentityKey: {GetShortByteString(response.X509IdentityKey)}");

            Console.WriteLine("\nSending Keys to Server");
            Console.WriteLine("/EncryptedSessions POST");

            HandshakeObject handshake = new()
            {
                PublicKey = ratchet.PublicKey,
                Signature = ratchet.IdentitySignature,
                X509IdentityKey = ratchet.X509IdentityKey
            };

            var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(handshake), Encoding.UTF8, "application/json");

            var responseMessage = await client.PostAsync("https://localhost:5001/API_Sessions", content);

            HandshakeObject receivedKeys = Newtonsoft.Json.JsonConvert.DeserializeObject<HandshakeObject>(await responseMessage.Content.ReadAsStringAsync());

            Console.WriteLine($"Received Keys \nX509IdentityKey:{GetShortByteString(receivedKeys.X509IdentityKey)}\nPublicKey:{GetShortByteString(receivedKeys.PublicKey)}\nId:{receivedKeys.Id}");

            Console.WriteLine("\nCreating Secret");

            ratchet.TryCreateSharedSecret(response.X509IdentityKey, receivedKeys.PublicKey, receivedKeys.Signature);

            Console.WriteLine($"Created Secret PrivateKey: {GetShortByteString(ratchet.PrivateKey)}");

            // TEST AUTHENTICATION

            AuthenticationRequestModel request = new()
            {
                Id = TestingAccountId,
                OAuth = OAuthTestingKey
            };

            string dataToEncrypt = Newtonsoft.Json.JsonConvert.SerializeObject(request);

            if (symmetricHandler.TryEncrypt(dataToEncrypt, ratchet.PrivateKey, out EncryptedDataModel encryptedData))
            {
                if (ratchet.TrySignKey(encryptedData.Data, out byte[] Signature))
                {
                    encryptedData.Signature = Signature;

                    EncryptedMessageModel message = new()
                    {
                        Id = receivedKeys.Id,
                        EncryptedData = encryptedData
                    };

                    content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");

                    Console.WriteLine("\nSending API_Authentication POST to authorize OAuth Token");

                    responseMessage = await client.PostAsync("https://localhost:5001/API_Authentication", content);

                    Console.WriteLine($"Got Response Code {responseMessage.StatusCode}");
                }
                else
                {
                    Console.WriteLine("Failed to sign encrypted data");
                }
            }
            else
            {
                Console.WriteLine("Failed to encryopt data");
            }

            // END TEST AUTHENTICATION

            // START TEST SEND MESSAGE


            // END TEST SEND MESSAGE
        }

        private class Client
        {

        }

        public static TmpLogger<DiffieHellmanRatchet> logger = new();
        public static TmpLogger<DiffieHellmanHandler> dhLogger = new();
        public static TmpLogger<SignatureHandler> sLogger = new();
        public static TmpLogger<SymmetricHandler<EncryptedDataModel>> smLogger = new();
        public static TmpLogger<KeyDerivationRatchet<EncryptedDataModel>> kdfLogger = new();
        private static IDiffieHellmanRatchet CreateRatchet()
        {
            return new DiffieHellmanRatchet(logger, new DiffieHellmanHandler(dhLogger), new KeyDerivationFunction(), new SignatureHandler(sLogger));

        }

        #region Helpers

        public static byte[] Combine(params byte[][] arrays)
        {
            // thanks jon skeet
            byte[] ret = new byte[arrays.Sum(x => x.Length)];
            int offset = 0;
            foreach (byte[] data in arrays)
            {
                Buffer.BlockCopy(data, 0, ret, offset, data.Length);
                offset += data.Length;
            }
            return ret;
        }

        private static Task Wait(int ms = 100)
        {
            return Task.Run(() => Thread.Sleep(ms));
        }

        private static void LogKey(byte[] PublicKey, byte[] PrivateKey, string Line1Header = "Public Key ", string Line2Header = "Private Key")
        {
            string key1 = PublicKey is null ? "none" : GetByteString(PublicKey ?? null)?[..40] ?? "none";
            string key2 = PrivateKey is null ? "none" : GetByteString(PrivateKey ?? null)?[..40] ?? "none";
            int len1 = PublicKey?.Length ?? 0;
            int len2 = PrivateKey?.Length ?? 0;
            Console.WriteLine("{2}: {0,-40} ({1})", key1, len1, Line1Header);
            Console.WriteLine("{2}: {0,-40} ({1})", key2, len2, Line2Header);
        }

        /// <summary>
        /// Converts byte[] to readable string for consoles
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private static string GetByteString(byte[] b)
        {
            if (b is null)
            {
                return "null";
            }
            string format = "";
            for (int i = 0; i < b.Length; i++)
            {
                format += "{" + i + ",4}";
            }
            return string.Format(format, b.Select(x => x.ToString()).ToArray());
        }

        private static string GetShortByteString(byte[] b)
        {
            string s = GetByteString(b ?? null)?[..40] ?? "none";
            return string.Format("{0,-40} ({1})", s, b?.Length);
        }
        #endregion Helpers
    }
}