using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

        public async Task<string> GetState(string Id, string FriendId)
        {
            if (Helpers.FullVerifyGuid(ref Id, logger) is false)
            {
                return null;
            }

            try
            {
                // states are stored as a dictionary with friend ids and states


                Dictionary<string, string> states = await GetAllStates(Id);

                if (states.ContainsKey(FriendId))
                {
                    return states[FriendId];
                }

                return null;
            }
            catch (Exception e)
            {
                logger.LogError("Failed to set state for {Id} {Error}", Id, e);
                return null;
            }
        }

        public async Task<bool> SetState(string Id, string FriendId, string State)
        {
            if (Helpers.FullVerifyGuid(ref Id, logger) is false)
            {
                return false;
            }
            if (Helpers.FullVerifyGuid(ref FriendId, logger) is false)
            {
                return false;
            }

            // we want to avoid sql injection but we are limited as the state of encryption clients contains intricate JSON and we dont want to accidentally nuke the states
            State = State.Replace(";", "");
            State = State.Replace("'", "");

            try
            {
                Dictionary<string, string> states = await GetAllStates(Id);

                if (states.ContainsKey(FriendId))
                {
                    states[FriendId] = State;
                }
                else
                {
                    states.Add(FriendId, State);
                }

                return await SetAllStates(Id, states);
            }
            catch (Exception e)
            {
                logger.LogError("Failed to set state for {Id} {Error}", Id, e);
                return false;
            }
        }

        private async Task<Dictionary<string, string>> GetAllStates(string Id)
        {
            try
            {
                // states are stored as a dictionary with friend ids and states

                string serializedStates = await db.ExecuteSingleProcedure<string, dynamic>(GetStateProcedureName, new { Id });

                if (string.IsNullOrEmpty(serializedStates))
                {
                    return new();
                }

                Dictionary<string, string> states = JsonConvert.DeserializeObject<Dictionary<string, string>>(serializedStates);

                return states ?? new();
            }
            catch (Exception e)
            {
                logger.LogError("Failed to set state for {Id} {Error}", Id, e);
                return new();
            }
        }

        private async Task<bool> SetAllStates(string Id, Dictionary<string, string> states)
        {
            try
            {
                // states are stored as a dictionary with friend ids and states
                string serializedStates = JsonConvert.SerializeObject(states);

                await db.ExecuteVoidProcedure<dynamic>(SetStateProcedureName, new { Id, EncryptionClientState = serializedStates });

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
