using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DingoAPI.Models
{
    public class EncryptedSessionModel : IEncryptedSessionModel
    {
        /// <summary>
        /// Used to verify the signature of any incoming encrypted data
        /// </summary>
        public byte[] X509IdentityKey { get; set; }
        public byte[] AsymmetricKey { get; set; }
    }
}
