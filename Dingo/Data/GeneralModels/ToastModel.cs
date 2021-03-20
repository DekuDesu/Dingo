using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dingo.Data.GeneralModels
{
    public class ToastModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime TimeSent { get; set; } = DateTime.UtcNow;
        public Func<Task> OnClose { get; set; } = null;

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(20);

        public bool Remove { get; set; } = false;
    }
}
