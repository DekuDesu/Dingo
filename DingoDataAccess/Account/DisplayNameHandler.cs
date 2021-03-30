using DingoDataAccess.Models.Friends;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
                // check to make sure the unique id for this person isn't taken for the next username they are switching to
                if (await UniqueIdentifierAvailable(newDisplayName, myFriendInfo.UniqueIdentifier) is false)
                {
                    // since the indentifier was taken get another one and then set it
                    short? newUniqueId = await GetAvailableUniqueIdentifier(newDisplayName);

                    if (newUniqueId is null)
                    {
                        logger.LogError("Failed to change for {Id} Name: {NewDisplayName} null unique Id retrieved", Id, newDisplayName);
                        return false;
                    }

                    await SetUniqueIdentifier(myFriendInfo.Id, (short)newUniqueId);
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

            await db.ExecuteVoidProcedure(SetUniqueIdentifiersProceduree, new { Id, UniqueIdentifier });

            return true;
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

            // try to prevent sql
            Helpers.CleanInputBasic(ref FormattedDisplayName);

            if (Helpers.TrySeperateDisplayName(ref FormattedDisplayName, out var result))
            {
                string DisplayName = result.DisplayName;

                List<TFullDisplayNameType> displayNames = await db.ExecuteProcedure<TFullDisplayNameType, dynamic>(SearchForFriendProcedure, new { DisplayName });

                if (displayNames?.Count is null or 0 || displayNames.Count > 1)
                {
                    return null;
                }

                if (displayNames[0].UniqueIdentifier == result.UniqueIdentifier)
                {
                    return displayNames[0].Id;
                }
            }

            return null;
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
                List<string> found = await db.ExecuteProcedure<string, dynamic>(UniqueIdentifierAvailableProcedure, new { DisplayName, UniqueIdentifier });

                if (found?.Count is null or 0)
                {
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                logger.LogError("Failed to check if identifier take Error:{Error}", e);
                return false;
            }
        }
    }
}
