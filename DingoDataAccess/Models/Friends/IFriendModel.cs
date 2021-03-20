using DingoDataAccess.Enums;
using System;

namespace DingoDataAccess.Models.Friends
{
    public interface IFriendModel
    {
        /// <summary>
        /// The Id used as the primary key in Dingo SQL databases and auth databases
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// The uri path to the avatar of the user
        /// </summary>
        string AvatarPath { get; set; }

        /// <summary>
        /// The name that should be displayed on the UI for this user
        /// </summary>
        string DisplayName { get; set; }

        /// <summary>
        /// The actual OnlineStatus of the user, this is either Online or Offline
        /// </summary>
        OnlineStatus Status { get; set; }

        /// <summary>
        /// The virtual status of the user, this is the status that the user selected to show to other users regardless of their actual OnlineStatus
        /// </summary>
        OnlineStatus VirtualStatus { get; set; }

        /// <summary>
        /// The 4 digit identifier of the user
        /// </summary>
        short UniqueIdentifier { get; set; }
    }
}