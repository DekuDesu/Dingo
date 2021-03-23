using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dingo
{

    public class MessageCallbackReference
    {
        public string FriendId { get; set; }
        public Func<Task> Callback { get; set; }
    }
}
