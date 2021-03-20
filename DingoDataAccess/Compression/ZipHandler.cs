using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DingoDataAccess.Compression
{
    public class ZipHandler : IZipHandler
    {
        public Encoding Encoder = new UnicodeEncoding();

        private void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        /// <summary>
        /// Zips a string using GZIP, default byte[] encoding is encoded to Unicode
        /// </summary>
        /// <param name="stringToZip"></param>
        /// <returns></returns>
        public byte[] Zip(string stringToZip)
        {
            var bytes = Encoder.GetBytes(stringToZip);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    //msi.CopyTo(gs);
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        /// <summary>
        /// unzips a byte[] using gzip, by default encodes into Unicode
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    //gs.CopyTo(mso);
                    CopyTo(gs, mso);
                }

                return Encoder.GetString(mso.ToArray());
            }
        }

    }
}
