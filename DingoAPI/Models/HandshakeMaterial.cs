using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DingoAPI.Models
{
    public class HandshakeMaterial
    {
        public byte[] X509IdentityKey { get; set; }
        public byte[] Signature { get; set; }
        public byte[] PublicKey { get; set; }
        public string Id { get; set; }
    }
}
