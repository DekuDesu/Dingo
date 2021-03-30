using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dingo.Data.GeneralModels
{
    public class ModalModel
    {
        public string Title { get; set; } = "Confirm";

        public dynamic Content { get; set; }

        public string CancelText { get; set; } = "Cancel";

        public string SubmitText { get; set; } = "Continue";

        public Func<Task> SubmitCallback { get; set; }

        public Func<Task> CancelCallback { get; set; }

        public bool DisplayTextAsRawHTML { get; set; } = false;
    }
}
