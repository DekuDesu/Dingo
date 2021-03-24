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
        /// <summary>
        /// The API session Id provided using API_Sessions
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Data that has been encrypted using the shared secret created using API_Sessions
        /// </summary>
        public EncryptedDataModel EncryptedData { get; set; }
    }
}
