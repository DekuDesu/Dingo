using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DingoDataAccess
{
    public class MessageModel : IMessageModel
    {
        public string SenderId { get; set; }
        public DateTime TimeSent { get; set; } = DateTime.UtcNow;
        public string Message { get; set; }
    }
}
