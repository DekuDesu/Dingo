using System;
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
    public sealed class TopLevelObjects : ITopLevelObjects
    {
        /// <summary>
        /// When added to displays and consumes the toast
        /// </summary>
        public readonly List<ToastModel> toasts = new();

        /// <summary>
        /// When added to displays and consums the modal
        /// </summary>

        public readonly List<ModalModel> modals = new();

        /// <summary>
        /// The current friends of the user, is updated on log-in and checked periodically automatically
        /// </summary>

        public List<IFriendModel> Friends { get; set; } = new();

        /// <summary>
        /// When ran will update the the Friends list on this object
        /// </summary>

        public Func<Task> GetFriends { get; set; }

        /// <summary>
        /// List of the waiting friend requests for this user
        /// </summary>

        public List<IFriendModel> FriendRequests { get; set; } = new();

        /// <summary>
        /// Refreshes the friends requests manually
        /// </summary>
        public Func<Task> GetFriendRequests { get; set; }

        /// <summary>
        /// The list of friends that have unread messages for the user, auto-updates
        /// </summary>
        public List<IFriendModel> Messages { get; set; } = new();

        /// <summary>
        /// Force updates the Messages
        /// </summary>
        public Func<Task> GetMessages { get; set; }

        /// <summary>
        /// List of notifications that have been already sent to the user
        /// </summary>

        public ConcurrentDictionary<string, int> AlreadyPushedMessageIds { get; set; } = new();

        /// <summary>
        /// List of toasts already pushed to the user for the given ids
        /// </summary>

        public List<string> AlreadyPushedRequestIds { get; set; } = new();

        /// <summary>
        /// When invoked force updates top level state of the site
        /// </summary>
        public Action StateHasChanged { get; set; }

        /// <summary>
        /// When  true the screen will shwo the change avatar modal
        /// </summary>

        public bool ShowChangeAvatar { get; set; } = false;

        /// <summary>
        /// When true the screen will show the change name modal
        /// </summary>

        public bool ShowChangeDisplayName { get; set; } = false;

        /// <summary>
        /// When set to true the message interface will open up for whatever the current chat id is
        /// </summary>

        public bool ShowChatInterface { get; set; } = false;

        /// <summary>
        /// The Id of the user that this user should chat with then the chat interface is shown
        /// </summary>
        public string CurrentChatId { get; set; } = null;

        public event Action BeforeDispose;

        public ILogger<TopLevelObjects> logger { get; set; }

        /// <summary>
        /// Dictionary containing all the timers that are running for this user
        /// </summary>

        private readonly ConcurrentTimerDictionary<ILogger<TopLevelObjects>> TimerDict;

        public TopLevelObjects()
        {
            TimerDict = new(logger)
            {
                VerboseLogging = true
            };
            // tell all timers added to the timer dict to force-update UI state when they invoke
            TimerDict.OnTimer = StateHasChanged;
        }

        public Task<string> AddTimer(int RefreshRate, Func<Task> Callback, string Key = null)
        {
            TimerDict.logger ??= logger;
            TimerDict.OnTimer = StateHasChanged;
            return TimerDict.AddTimer(RefreshRate, Callback, Key);
        }

        public Task RemoveTimer(string Key)
        {
            return TimerDict.RemoveTimer(Key);
        }

        public void Dispose()
        {
            // call the before dispose first to execute any cleanup code like setting the users status to offline ect..
            BeforeDispose?.Invoke();

            // dispose all the timers that are running when this object is disposed
            TimerDict?.Dispose();
        }
    }
}
