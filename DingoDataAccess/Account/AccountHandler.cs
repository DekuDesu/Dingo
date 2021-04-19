using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DingoAuthentication.Encryption;

namespace DingoDataAccess.Account
{
    public class AccountHandler : IAccountHandler
    {
        private readonly ISqlDataAccess db;
        private readonly ISqlDataAccess messagesDb;
        private readonly ILogger<AccountHandler> logger;
        private readonly IDisplayNameHandler displayNameHandler;
        //private readonly IDiffieHellmanHandler diffieHellmanHandler;
        //private readonly IKeyAndBundleHandler<TKeyBundleType, TSignedKeyModelType> bundleHandler;
        private const string ConnectionStringName = "DingoUsersConnection";
        private const string MessagesConnectionStringName = "DingoMessagesConnection";

        private const string CreateNewUserProcedure = "CreateNewUser";
        private const string DeleteUserProcedure = "DeleteUser";

        private const string MessagesCreateNewUserProcedure = "CreateUser";
        private const string MessagesDeleteUserProcedure = "DeleteUser";

        public AccountHandler(ISqlDataAccess _db, ISqlDataAccess _messagesDb, ILogger<AccountHandler> _logger, IDisplayNameHandler _displayNameHandler)
        {
            db = _db;
            messagesDb = _messagesDb;
            logger = _logger;
            displayNameHandler = _displayNameHandler;

            // this is used to create the identity keys for this account
            //this.diffieHellmanHandler = diffieHellmanHandler;
            // this is used to save the keys created
            //this.bundleHandler = bundleHandler;

            db.ConnectionStringName = ConnectionStringName;
            messagesDb.ConnectionStringName = MessagesConnectionStringName;
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

            // create the user in the message DB as well
            await messagesDb.ExecuteVoidProcedure(MessagesCreateNewUserProcedure, new { Id });

            // create identity keys
            //var (PublicKey, PrivateKey) = diffieHellmanHandler.GenerateKeys();

            // store the keys
            //await bundleHandler.SetKeys(Id, PublicKey, PrivateKey);

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

            await messagesDb.ExecuteVoidProcedure(MessagesDeleteUserProcedure, new { Id });

            logger.LogInformation("Deleted account for {Id}", Id);

            return true;
        }
    }
}
