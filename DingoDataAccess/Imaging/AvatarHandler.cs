using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using Microsoft.Extensions.Logging;

namespace DingoDataAccess
{
    /// <summary>
    /// Converts and stores images in the user database
    /// </summary>
    public class AvatarHandler : IAvatarHandler
    {
        private readonly ISqlDataAccess db;
        private readonly ILogger<AvatarHandler> logger;

        private const string ConnectionStringName = "DingoUsersConnection";

        private const string GetAvatarProcedure = "GetAvatar";
        private const string SetAvatarProcedure = "SetAvatar";

        public AvatarHandler(ISqlDataAccess _db, ILogger<AvatarHandler> _logger)
        {
            db = _db;
            logger = _logger;
            _db.ConnectionStringName = ConnectionStringName;
        }

        public async Task<bool> SetAvatar(string Id, string Base64Avatar)
        {
            if (Helpers.FullVerifyGuid(ref Id, logger) is false)
            {
                return false;
            }

            try
            {
                await db.ExecuteVoidProcedure(SetAvatarProcedure, new { Id, Avatar = Base64Avatar });

                return true;
            }
            catch (Exception e)
            {
                logger.LogError("Failed to set Avatar for {Id} Error: {Error}", Id, e);
                return false;
            }
        }

        public async Task<string> GetAvatar(string Id)
        {
            if (Helpers.FullVerifyGuid(ref Id, logger) is false)
            {
                return null;
            }

            try
            {
                return await db.ExecuteSingleProcedure<string, dynamic>(GetAvatarProcedure, new { Id });
            }
            catch (Exception e)
            {
                logger.LogError("Failed to get Avatar for {Id} Error: {Error}", Id, e);
                return null;
            }
        }
    }
}
