using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DingoDataAccess.OAuth
{
    public class OAuthHandler
    {
        private readonly ISqlDataAccess db;
        private readonly ILogger<OAuthHandler> logger;

        private const string ConnectionStringName = "DingoUsersConnection";

        private const string GetOAuthProcedureName = "GetOAuth";

        private const string SetOAuthProcedureName = "SetOAuth";

        private const string UpdateOAuthProcedureName = "UpdateOAuth";

        public OAuthHandler(ISqlDataAccess db, ILogger<OAuthHandler> _logger)
        {
            this.db = db;
            logger = _logger;
            this.db.ConnectionStringName = ConnectionStringName;
        }

        public async Task<string> GetOAuth(string Id)
        {
            // make sure to avoid possible Sql Injection
            if (Helpers.FullVerifyGuid(ref Id, logger) is false)
            {
                return null;
            }

            string OAuth = await db.ExecuteSingleProcedure<string, dynamic>(GetOAuthProcedureName, new { Id });

            logger.LogInformation("Retrieved OAuth for {Id}: {OAuth}", Id, OAuth);

            return OAuth;
        }

        public Task<bool> SetOAuth(string Id, string OAuth) => ExecuteTwoParamOAuthQuery(Id, OAuth, SetOAuthProcedureName);

        public Task<bool> UpdateOAuth(string Id, string OAuth) => ExecuteTwoParamOAuthQuery(Id, OAuth, UpdateOAuthProcedureName);

        private async Task<bool> ExecuteTwoParamOAuthQuery(string Id, string OAuth, string ProcedureName)
        {
            // make sure to avoid possible Sql Injection
            if (Helpers.FullVerifyGuid(ref Id, logger) is false)
            {
                return false;
            }
            // make sure to avoid possible Sql Injection
            if (Helpers.FullVerifyGuid(ref OAuth, logger) is false)
            {
                return false;
            }

            try
            {
                await db.ExecuteVoidProcedure(ProcedureName, new { Id, OAuth });

                return true;
            }
            catch (Exception e)
            {
                logger.LogError("Failed to {ProcedureName} OAuth for {Id} Oath: {OAuth} Error: {Error}", ProcedureName, Id, OAuth, e);
                return false;
            }
        }
    }
}
