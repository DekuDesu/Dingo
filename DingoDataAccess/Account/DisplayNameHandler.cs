using DingoDataAccess.Models.Friends;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DingoDataAccess.Account
{
    /// <summary>
    /// Changes te D
    /// </summary>
    public class DisplayNameHandler<TFullDisplayNameType> : IDisplayNameHandler where TFullDisplayNameType : IFullDisplayNameModel, new()
    {
        private readonly ILogger logger;
        private readonly ISqlDataAccess db;
        private readonly IFriendHandler friendHandler;
        internal static QueryCache<string, dynamic> Cache;

        public string ConnectionStringName = "DingoUsersConnection";

        const string GetDisplayNameProcedure = "GetDisplayName";

        const string SetDisplayNameProcedure = "SetDisplayName";

        const string ChangeDisplayNameProcedure = "ChangeDisplayName";

        const string GetUniqueIdentifiersProcedure = "GetUniqueIdentifiersWithDisplayName";

        const string SetUniqueIdentifiersProceduree = "SetUniqueIdentifier";

        const string SearchForFriendProcedure = "SearchForFriend";

        const string UniqueIdentifierAvailableProcedure = "UniqueIdentifierAvailable";

        const string FindIdByNameHashProcedure = "FindIdByNameHash";

        public DisplayNameHandler(ILogger<DisplayNameHandler<TFullDisplayNameType>> _Logger, ISqlDataAccess _db, ILogger<QueryCache<string, dynamic>> cacheLogger, IFriendHandler friendHandler)
        {
            logger = _Logger;
            db = _db;
            this.friendHandler = friendHandler;
            db.ConnectionStringName = ConnectionStringName;
            Cache = new(cacheLogger);
        }

        public async Task<string> GetDisplayName(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return null;
            }

            // make sure the id given is actuall an id
            if (Helpers.FullVerifyGuid(ref Id, logger) is false)
            {
                return null;
            }

            // clean any possible sql
            Helpers.CleanInputBasic(ref Id);

            logger.LogInformation("Fectching username for {UserId}", Id);

            string result;

            dynamic parameters = new { Id };

            if ((result = await Cache.GetCachedOrDefault(GetDisplayNameProcedure, parameters)) != default)
            {
                logger.LogInformation("Fetched cached username {Username} for {Id}", result, Id);
                return result;
            }

            result = await db.ExecuteSingleProcedure<string, dynamic>(GetDisplayNameProcedure, new { Id });

            if (result != default)
            {
                await Cache.UpdateOrCache(GetDisplayNameProcedure, parameters, result);
            }

            return result;
        }

        public async Task<bool> SetDisplayName(string Id, string newDisplayName)
        {
            // make sure the user isn't trying to inject via the id
            if (Helpers.FullVerifyGuid(ref Id, logger) is false)
            {
                return false;
            }

            // clean any possible sql
            Helpers.CleanInputBasic(ref newDisplayName);

            if (string.IsNullOrEmpty(newDisplayName))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(newDisplayName))
            {
                return false;
            }

            // make sure the display name has no leading or trailing whitespace
            newDisplayName = newDisplayName.Trim();

            // get the old display name to check to see if we need to update or set the name
            IFriendModel myFriendInfo = await friendHandler.GetFriend(Id);

            bool alreadyAdded = myFriendInfo != default;

            string storedProcedure;

            dynamic parameters = new { Id, DisplayName = newDisplayName };

            if (alreadyAdded)
            {
                logger.LogInformation("Changing displayname from {OldDisplayName} to {NewDisplayName} for {Id}", myFriendInfo.DisplayName, newDisplayName, Id);
                storedProcedure = ChangeDisplayNameProcedure;
            }
            else
            {
                logger.LogInformation("Setting new display name for {Id} Name: {NewDisplayName}", Id, newDisplayName);
                storedProcedure = SetDisplayNameProcedure;
            }

            try
            {
                if (myFriendInfo != null)
                {
                    // check to make sure the unique id for this person isn't taken for the next username they are switching to
                    if (await UniqueIdentifierAvailable(newDisplayName, myFriendInfo.UniqueIdentifier) is false)
                    {
                        await SetRandomUniqueIdentifier(Id, newDisplayName);
                    }
                }
                else
                {
                    await SetRandomUniqueIdentifier(Id, newDisplayName);
                }

                // change or set the name
                await db.ExecuteProcedure<dynamic, dynamic>(storedProcedure, parameters);

                // update the name in cache
                await Cache.UpdateOrCache(GetDisplayNameProcedure, new { Id }, newDisplayName);

                return true;
            }
            catch (Exception e)
            {
                logger.LogError("Failed to change for {Id} Name: {NewDisplayName} null unique Id retrieved Error: {Error}", Id, newDisplayName, e);

                return false;
            }
        }

        public async Task<short?> GetAvailableUniqueIdentifier(string DisplayName)
        {
            if (string.IsNullOrEmpty(DisplayName))
            {
                return null;
            }

            // reduce chance of sql injection
            Helpers.CleanInputBasic(ref DisplayName);

            List<short> identifiers = await db.ExecuteProcedure<short, dynamic>(GetUniqueIdentifiersProcedure, new { DisplayName });

            if (identifiers?.Count is null or 0)
            {
                return null;
            }

            return await RandomNumberGenerator.NextUnique(identifiers, 1000, 10000);
        }

        public async Task<bool> SetUniqueIdentifier(string Id, short UniqueIdentifier)
        {
            if (Helpers.FullVerifyGuid(ref Id, logger) is false)
            {
                return false;
            }
            try
            {
                await db.ExecuteVoidProcedure(SetUniqueIdentifiersProceduree, new { Id, UniqueIdentifier });

                return true;
            }
            catch (Exception e)
            {
                logger.LogError("Failed to set unique identitfier for {id}#{num} Error: {Error}", Id, UniqueIdentifier, e);
                return false;
            }

        }

        /// <summary>
        /// Gets the id from a display name that's in format DisplayName#UniqueIdentifier
        /// </summary>
        /// <param name="FormattedDisplayName"></param>
        /// <returns></returns>
        public async Task<string> GetIdFromDisplayName(string FormattedDisplayName)
        {
            if (FormattedDisplayName?.Length is null or 0)
            {
                return null;
            }

            try
            {
                // hash the name with sha256 and look it up, it's hashed to prevent sql injection since this is one of the few entry points to the db that needs user input
                var ids = await GetIdsWithName(FormattedDisplayName);

                // when the name was hashed it matched no values
                if (ids.Count is 0)
                {
                    return null;
                }

                // when hashed it only matched one hash
                if (ids.Count is 1)
                {
                    return ids[0];
                }

                // since multiple names hashed to the same value determine if any of them match the provided name
                foreach (var item in ids)
                {
                    if (await GetDisplayName(item) == FormattedDisplayName)
                    {
                        return item;
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                logger.LogError("Failed to find friend with name {FormattedDisplayName} Error: {Error}", FormattedDisplayName, e);
                return null;
            }
        }

        /// <summary>
        /// Determines if the identifier for the given name has been taken
        /// </summary>
        /// <param name="DisplayName"></param>
        /// <param name="UniqueIdentitfier"></param>
        /// <returns></returns>
        private async Task<bool> UniqueIdentifierAvailable(string DisplayName, short UniqueIdentifier)
        {
            try
            {
                var found = await GetIdsWithName(DisplayName, UniqueIdentifier);

                return found?.Count is null or 0;
            }
            catch (Exception e)
            {
                logger.LogError("Failed to check if identifier take Error:{Error}", e);
                return false;
            }
        }

        private async Task<bool> SetRandomUniqueIdentifier(string Id, string DisplayName)
        {
            try
            {
                // since the indentifier was taken get another one and then set it
                short? newUniqueId = await GetAvailableUniqueIdentifier(DisplayName);

                if (newUniqueId is null)
                {
                    logger.LogError("Failed to change for {Id} Name: {NewDisplayName} null unique Id retrieved", Id, DisplayName);
                    return false;
                }

                await SetUniqueIdentifier(Id, (short)newUniqueId);

                return true;
            }
            catch (Exception e)
            {
                logger.LogError("Failed to change for {Id} Name: {NewDisplayName} Error: {Error}", Id, DisplayName, e);
                return false;
            }
        }

        /// <summary>
        /// Concats then Hashes the input and finds it in the DB
        /// </summary>
        /// <param name="DisplayNameAndIdentifier"></param>
        /// <returns></returns>
        private Task<List<string>> GetIdsWithName(string DisplayName, short UniqueIdentifier) => GetIdsWithName($"{DisplayName}#{UniqueIdentifier}");

        /// <summary>
        /// Hashes the input and finds it in the DB
        /// </summary>
        /// <param name="DisplayNameAndIdentifier"></param>
        /// <returns></returns>
        private async Task<List<string>> GetIdsWithName(string DisplayNameAndIdentifier)
        {
            try
            {
                // hash the name of sha256
                string NameHash = Encoding.Unicode.GetString(SHA256.HashData(Encoding.Unicode.GetBytes(DisplayNameAndIdentifier)));

                return await db.ExecuteProcedure<string, dynamic>(FindIdByNameHashProcedure, new { NameHash }) ?? new();
            }
            catch (Exception e)
            {
                logger.LogError("Failed to get Id for name {Name} Error: {Error}", DisplayNameAndIdentifier, e);
                return new();
            }
        }
    }
}
