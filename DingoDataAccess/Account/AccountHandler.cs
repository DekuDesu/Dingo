using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DingoDataAccess.Account
{
    public class AccountHandler : IAccountHandler
    {
        private readonly ISqlDataAccess db;
        private readonly ILogger<AccountHandler> logger;
        private readonly IDisplayNameHandler displayNameHandler;

        private const string ConnectionStringName = "DingoUsersConnection";

        private const string CreateNewUserProcedure = "CreateNewUser";
        private const string DeleteUserProcedure = "DeleteUser";

        public AccountHandler(ISqlDataAccess _db, ILogger<AccountHandler> _logger, IDisplayNameHandler _displayNameHandler)
        {
            db = _db;
            logger = _logger;
            displayNameHandler = _displayNameHandler;
            db.ConnectionStringName = ConnectionStringName;
        }

        /// <summary>
        /// Sets the display name and creates a friends list entry
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="DisplayName"></param>
        /// <returns></returns>
        public async Task<bool> CreateNewAccount(string Id, string DisplayName)
        {
            // try to set the display name for the new user
            if (await displayNameHandler.SetDisplayName(Id, DisplayName) is false)
            {
                logger.LogError("Failed to set the display name of a new user, Name: {DisplayName} Id: {Id}", DisplayName, Id);
                return false;
            }

            // try to get an available tag for the player
            short? result = await displayNameHandler.GetAvailableUniqueIdentifier(DisplayName);

            if (result is short UniqueIdentifier)
            {
                // since there was one available set it
                await displayNameHandler.SetUniqueIdentifier(Id, UniqueIdentifier);
            }
            else
            {
                // short was malformed, outside of bounds, or not available
                logger.LogError("Failed to get short to create a new user, Name: {DisplayName} Id: {Id} UniqueId: {UniqueIdentifier}", DisplayName, Id, result);
                return false;
            }

            // create the friends list, blocked list, and request list for the user
            await db.ExecuteVoidProcedure(CreateNewUserProcedure, new { Id });

            logger.LogInformation("Finished creating new user {DisplayName}#{result} {Id}", DisplayName, result, Id);

            return true;
        }

        public async Task<bool> DeleteAccount(string Id)
        {
            if (Helpers.FullVerifyGuid(ref Id, logger) is false)
            {
                logger.LogWarning("Attempted to delete account with malformed id {Id}", Id);
                return false;
            }

            await db.ExecuteVoidProcedure(DeleteUserProcedure, new { Id });

            logger.LogInformation("Deleted account for {Id}", Id);

            return true;
        }
    }
}
