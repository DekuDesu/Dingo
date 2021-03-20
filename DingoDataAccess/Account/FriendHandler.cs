using DingoDataAccess.Models.Friends;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace DingoDataAccess.Account
{
    public class FriendHandler<TFriendModelType> : IFriendHandler where TFriendModelType : IFriendModel, new()
    {
        private readonly ISqlDataAccess db;
        private readonly ILogger<FriendHandler<TFriendModelType>> logger;
        private readonly IFriendListHandler friendListHandler;
        private static QueryCache<TFriendModelType, dynamic> Cache;

        private const string DatabaseConnectionName = "DingoUsersConnection";

        private const string GetFriendProcedure = "GetFriend";

        public FriendHandler(ISqlDataAccess db, ILogger<FriendHandler<TFriendModelType>> _logger, ILogger<QueryCache<TFriendModelType, dynamic>> cacheLogger, IFriendListHandler _friendListHandler)
        {
            this.db = db;
            logger = _logger;
            friendListHandler = _friendListHandler;
            this.db.ConnectionStringName = DatabaseConnectionName;
            Cache = new(cacheLogger);
        }

        public Task<List<IFriendModel>> GetBlocked(string Id) => GetFriendModelList(Id, friendListHandler.GetBlockedIds);

        public Task<List<IFriendModel>> GetRequests(string Id) => GetFriendModelList(Id, friendListHandler.GetRequestIds);

        public Task<List<IFriendModel>> GetFriendList(string Id) => GetFriendModelList(Id, friendListHandler.GetFriendIds);

        /// <summary>
        /// Gets a list of IFriend models from any function that is an async Task<List<string>>
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="ListSource"></param>
        /// <returns></returns>
        private async Task<List<IFriendModel>> GetFriendModelList(string Id, Func<string, Task<List<string>>> ListSource)
        {
            List<IFriendModel> friends = new();

            // make sure the id is valid
            if (Helpers.FullVerifyGuid(ref Id, logger) is false)
            {
                return friends;
            }

            var friendIds = await ListSource(Id);

            logger.LogInformation("Found {NumberOfFriends} friends for {Id}", friendIds.Count, Id);

            if (friendIds?.Count is null or 0)
            {
                return friends;
            }

            foreach (var item in friendIds)
            {
                IFriendModel foundFriend = await GetFriend(item);

                if (foundFriend != null)
                {
                    logger.LogInformation("Added friend {FriendModel}", foundFriend);
                    friends.Add(foundFriend);
                }
            }

            logger.LogInformation("Retrived friends list for {Id} List: {RawList}", Id, friends);

            return friends;
        }

        public async Task<IFriendModel> GetFriend(string Id)
        {
            logger.LogInformation("Fetching friend model for: {Id}", Id);

            // make sure the id we got is a valid Guid
            if (Helpers.FullVerifyGuid(ref Id, logger) is false)
            {
                return null;
            }

            // limit sql injections as much as possible
            Helpers.CleanInputBasic(ref Id);

            var parameters = new { Id };

            TFriendModelType result;

            if ((result = await Cache.GetCachedOrDefault(GetFriendProcedure, parameters)) != null)
            {
                return result;
            }

            result = await db.ExecuteSingleProcedure<TFriendModelType, dynamic>(GetFriendProcedure, parameters);

            await Cache.UpdateOrCache(GetFriendProcedure, parameters, result);

            return result;
        }

        public Task<bool> AddFriend(string Id, string IdToAdd) => AddRemoveFriend(Id, IdToAdd, true);

        public Task<bool> RemoveFriend(string Id, string IdToRemove) => AddRemoveFriend(Id, IdToRemove, false);

        public Task<bool> SendRequest(string Id, string IdToSendRequestTo) => AddRemoveRequest(IdToSendRequestTo, Id, true);

        public Task<bool> RemoveRequest(string Id, string IdToRemove) => AddRemoveRequest(Id, IdToRemove, false);

        public Task<bool> BlockFriend(string Id, string IdToBlock) => AddRemoveBlock(Id, IdToBlock, true);

        public Task<bool> UnblockFriend(string Id, string IdToUnBlock) => AddRemoveBlock(Id, IdToUnBlock, false);

        private async Task<bool> AddRemoveFriend(string Id, string IdToRemoveOrAdd, bool add)
        {
            logger.LogInformation("{ShouldAdd} {FriendId} as a friend to {Id}", add ? "Adding" : "Removing", IdToRemoveOrAdd, Id);

            // make sure the id we got is a valid Guid
            if (Helpers.FullVerifyGuid(ref Id, logger) is false)
            {
                return false;
            }

            // make sure the NewFriendId we got is a valid Guid
            if (Helpers.FullVerifyGuid(ref IdToRemoveOrAdd, logger) is false)
            {
                return false;
            }

            List<string> currentFriends = await friendListHandler.GetFriendIds(Id);

            currentFriends ??= new();

            if (add)
            {
                if (currentFriends.Contains(IdToRemoveOrAdd) is false)
                {
                    currentFriends.Add(IdToRemoveOrAdd);
                }
            }
            else
            {
                currentFriends.Remove(IdToRemoveOrAdd);
            }

            await friendListHandler.SetFriendIds(Id, currentFriends);

            return true;
        }

        private async Task<bool> AddRemoveRequest(string Id, string IdToAddOrRemove, bool add)
        {
            logger.LogInformation("{ShouldAdd} friend request {FriendId} from {Id}", add ? "Adding" : "Removing", IdToAddOrRemove, Id);

            // make sure the id we got is a valid Guid
            if (Helpers.FullVerifyGuid(ref Id, logger) is false)
            {
                return false;
            }

            // make sure the NewFriendId we got is a valid Guid
            if (Helpers.FullVerifyGuid(ref IdToAddOrRemove, logger) is false)
            {
                return false;
            }

            List<string> friendsRequests = await friendListHandler.GetRequestIds(Id);

            friendsRequests ??= new();

            if (add)
            {
                // make sure we aren't blocked by the Id
                List<string> blocked = await friendListHandler.GetBlockedIds(Id);

                if (blocked.Contains(IdToAddOrRemove))
                {
                    logger.LogInformation("Attempted to send request to blocked person");
                    return false;
                }

                if (friendsRequests.Contains(IdToAddOrRemove) is false)
                {
                    friendsRequests.Add(IdToAddOrRemove);
                }
            }
            else
            {
                friendsRequests.Remove(IdToAddOrRemove);
            }

            await friendListHandler.SetRequestIds(Id, friendsRequests);

            return true;
        }

        private async Task<bool> AddRemoveBlock(string Id, string IdToAddOrRemove, bool add)
        {
            logger.LogInformation("Blocking {FriendId} from {Id}", add ? "Adding" : "Removing", IdToAddOrRemove, Id);

            // make sure the id we got is a valid Guid
            if (Helpers.FullVerifyGuid(ref Id, logger) is false)
            {
                return false;
            }

            // make sure the NewFriendId we got is a valid Guid
            if (Helpers.FullVerifyGuid(ref IdToAddOrRemove, logger) is false)
            {
                return false;
            }

            List<string> blockedIds = await friendListHandler.GetBlockedIds(Id);

            blockedIds ??= new();

            if (add)
            {
                // if we are blocking this person remove them from the friends list if they're in there
                var friends = await friendListHandler.GetFriendIds(Id);

                if (friends.Contains(IdToAddOrRemove))
                {
                    await RemoveFriend(Id, IdToAddOrRemove);
                }

                // add to blocked list
                if (blockedIds.Contains(IdToAddOrRemove) is false)
                {
                    blockedIds.Add(IdToAddOrRemove);
                }
            }
            else
            {
                blockedIds.Remove(IdToAddOrRemove);
            }

            await friendListHandler.SetBlockedIds(Id, blockedIds);

            return true;
        }
    }
}
