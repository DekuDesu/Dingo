using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DingoDataAccess
{
    public class EncryptedClientStateHandler : IEncryptedClientStateHandler
    {
        private readonly ISqlDataAccess db;
        private readonly ILogger<EncryptedClientStateHandler> logger;

        private const string ConnectionStringName = "DingoMessagesConnection";

        private const string GetStateProcedureName = "GetEncryptionState";
        private const string SetStateProcedureName = "SetEncryptionState";

        public EncryptedClientStateHandler(ISqlDataAccess db, ILogger<EncryptedClientStateHandler> _logger)
        {
            this.db = db;
            logger = _logger;
            this.db.ConnectionStringName = ConnectionStringName;
        }

        public async Task<string> GetState(string Id)
        {
            if (Helpers.FullVerifyGuid(ref Id, logger) is false)
            {
                return null;
            }

            try
            {
                return await db.ExecuteSingleProcedure<string, dynamic>(GetStateProcedureName, new { Id });
            }
            catch (Exception e)
            {
                logger.LogError("Failed to set state for {Id} {Error}", Id, e);
                return null;
            }
        }

        public async Task<bool> SetState(string Id, string State)
        {
            if (Helpers.FullVerifyGuid(ref Id, logger) is false)
            {
                return false;
            }

            // we want to avoid sql injection but we are limited as the state of encryption clients contains intricate JSON and we dont want to accidentally nuke the states
            State = State.Replace(";", "");
            State = State.Replace("'", "");

            try
            {
                await db.ExecuteVoidProcedure<dynamic>(SetStateProcedureName, new { Id, EncryptionClientState = State });

                return true;
            }
            catch (Exception e)
            {
                logger.LogError("Failed to set state for {Id} {Error}", Id, e);
                return false;
            }
        }
    }
}
