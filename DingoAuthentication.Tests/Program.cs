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

namespace DingoAuthentication.Tests
{
    public class program
    {
        private static Encoding Encoder = new UnicodeEncoding();

        public static async Task Main()
        {
            Console.OutputEncoding = Encoding.Unicode;

            await Wait(100);

            //DiffieHellmanRatchet dhRatchet = new(new TmpLogger<DiffieHellmanRatchet>());

            Console.ReadLine();
            Console.ReadLine();
        }

        private class Client
        {

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