using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dingo.Data.GeneralModels;

namespace Dingo
{
    /// <summary>
    /// This holds references to the pertinent objects that are top level
    /// </summary>
    public class TopLevelObjects
    {
        public List<ToastModel> toasts = new();

        public List<ModalModel> modals = new();

        public Action StateHasChanged { get; set; }

        public bool ShowChangeAvatar = false;

        public bool ShowChangeDisplayName = false;
    }
}
