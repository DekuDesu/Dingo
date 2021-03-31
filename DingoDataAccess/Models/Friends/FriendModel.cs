using DingoDataAccess.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DingoDataAccess.Models.Friends
{
    /// <summary>
    /// Model that defines basic information about a user
    /// </summary>
    public class FriendModel : IFriendModel
    {
        public string Id { get; set; }

        public string DisplayName { get; set; }

        public string AvatarPath { get; set; }

        public OnlineStatus Status { get; set; }

        public OnlineStatus VirtualStatus { get; set; }

        public short UniqueIdentifier { get; set; }

        public string NameHash { get; set; }

        public override string ToString()
        {
            return $"Id: {Id} DisplayName: {DisplayName} Status: {Status} Virtual Status: {VirtualStatus} UniqueIdentifier: {UniqueIdentifier} AvatarPath: {AvatarPath}";
        }
    }
}
