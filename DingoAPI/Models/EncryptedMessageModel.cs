using DingoAuthentication.Encryption;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DingoAPI
{
    /// <summary>
    /// Associates an encrypted message with a session Id
    /// </summary>
    public class EncryptedMessageModel : IEncryptedMessageModel
    {
        public string Id { get; set; }
        public EncryptedDataModel EncryptedData { get; set; }
    }
}
