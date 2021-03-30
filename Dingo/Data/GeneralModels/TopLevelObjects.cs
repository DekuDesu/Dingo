using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Dingo.Data.GeneralModels;
using System.Threading.Tasks;
using System.Threading;
using DingoDataAccess.Models.Friends;
using DingoDataAccess;
using Microsoft.Extensions.Logging;

namespace Dingo
{
    /// <summary>
    /// This holds references to the pertinent objects that are top level
    /// </summary>
    public class TopLevelObjects : ITopLevelObjects
    {
        public readonly List<ToastModel> toasts = new();

        public readonly List<ModalModel> modals = new();

        public List<IFriendModel> Friends = new();

        public List<IFriendModel> FriendRequests = new();

        public Func<Task> GetFriendRequests { get; set; }

        public readonly List<string> WaitingSenderIds = new();

        public readonly List<string> WaitingRequestIds = new();

        public Action StateHasChanged { get; set; }

        public bool ShowChangeAvatar = false;

        public bool ShowChangeDisplayName = false;

        public bool ShowChatInterface = false;

        public string CurrentChatId { get; set; } = null;

        private readonly ConcurrentTimerDictionary TimerDict;

        public TopLevelObjects()
        {
            TimerDict = new();
            TimerDict.OnTimer = StateHasChanged;
        }

        public Task<string> AddTimer(int RefreshRate, Func<Task> Callback, string Key = null)
        {
            TimerDict.OnTimer = StateHasChanged;
            return TimerDict.AddTimer(RefreshRate, Callback, Key);
        }

        public Task RemoveTimer(string Key)
        {
            return TimerDict.RemoveTimer(Key);
        }

        public void Dispose()
        {
            TimerDict?.Dispose();
        }
    }
}
