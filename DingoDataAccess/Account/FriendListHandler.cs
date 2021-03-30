using DingoDataAccess.Models.Friends;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DingoDataAccess.Account
{
    public class FriendListHandler<TFriendModelType> : IFriendListHandler where TFriendModelType : IFriendModel, new()
    {
        public string ConnectionStringName { get; init; } = "DingoUsersConnection";

        private const string SetFriendsListProcedure = "SetFriendsList";
        private const string GetFriendsListProcedure = "GetFriendsList";

        private const string SetBlockedListProcedure = "SetBlockedList";
        private const string GetBlockedListProcedure = "GetBlockedList";

        private const string SetRequestListProcedure = "SetRequestList";
        private const string GetRequestListProcedure = "GetRequestList";

        private readonly ISqlDataAccess db;
        private readonly ILogger<FriendListHandler<TFriendModelType>> logger;

        public FriendListHandler(ISqlDataAccess db, ILogger<FriendListHandler<TFriendModelType>> _logger)
        {
            this.db = db;
            logger = _logger;
            db.ConnectionStringName = ConnectionStringName;
        }

        public async Task<List<string>> GetFriendIds(string Id)
        {
            List<string> friends = new();

            // make sure the id is valid
            if (Helpers.FullVerifyGuid(ref Id, logger) is false)
            {
                return friends;
            }

            var result = await db.ExecuteSingleProcedure<string, dynamic>(GetFriendsListProcedure, new { Id });

            if (result?.Length is null or 0)
            {
                return friends;
            }

            friends = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(result);

            return friends;
        }

        public async Task<bool> SetFriendIds(string Id, List<string> friendsList)
        {
            // make sure the id is valid
            if (Helpers.FullVerifyGuid(ref Id, logger) is false)
            {
                return false;
            }

            var FriendsList = Newtonsoft.Json.JsonConvert.SerializeObject(friendsList);

            await db.ExecuteProcedure<dynamic, dynamic>(SetFriendsListProcedure, new { Id, FriendsList });

            return true;
        }

        public async Task<bool> SetBlockedIds(string Id, List<string> blockedIds)
        {
            // make sure the id is valid
            if (Helpers.FullVerifyGuid(ref Id, logger) is false)
            {
                return false;
            }

            logger.LogInformation("Setting block list for {Id} List: {RawList}", Id, blockedIds);

            var BlockedList = Newtonsoft.Json.JsonConvert.SerializeObject(blockedIds);

            await db.ExecuteProcedure<dynamic, dynamic>(SetBlockedListProcedure, new { Id, BlockedList });

            return true;
        }

        public async Task<List<string>> GetBlockedIds(string Id)
        {
            List<string> blockedIds = new();

            // make sure the id is valid
            if (Helpers.FullVerifyGuid(ref Id, logger) is false)
            {
                return blockedIds;
            }

            var result = await db.ExecuteSingleProcedure<string, dynamic>(GetBlockedListProcedure, new { Id });

            if (result?.Length is null or 0)
            {
                return blockedIds;
            }

            blockedIds = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(result);

            return blockedIds;
        }

        public async Task<bool> SetRequestIds(string Id, List<string> requestIds)
        {
            // make sure the id is valid
            if (Helpers.FullVerifyGuid(ref Id, logger) is false)
            {
                return false;
            }

            var RequestList = Newtonsoft.Json.JsonConvert.SerializeObject(requestIds);

            await db.ExecuteProcedure<dynamic, dynamic>(SetRequestListProcedure, new { Id, RequestList });

            return true;
        }

        public async Task<List<string>> GetRequestIds(string Id)
        {
            List<string> requests = new();

            // make sure the id is valid
            if (Helpers.FullVerifyGuid(ref Id, logger) is false)
            {
                return requests;
            }

            var result = await db.ExecuteSingleProcedure<string, dynamic>(GetRequestListProcedure, new { Id });

            if (result?.Length is null or 0)
            {
                return requests;
            }

            requests = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(result);

            return requests;
        }
    }
}
