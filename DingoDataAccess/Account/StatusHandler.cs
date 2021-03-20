using DingoDataAccess.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DingoDataAccess.Account
{
    public class StatusHandler : IStatusHandler
    {
        private readonly ISqlDataAccess db;
        private readonly ILogger<StatusHandler> logger;

        private const string ConnectionStringName = "DingoUsersConnection";

        // the actual status of the user either Online or Offline
        private const string GetStatusProcedure = "GetStatus";
        private const string SetStatusProcedure = "SetStatus";

        // the status the user wants to display to other users
        private const string GetVirtualStatusProcedure = "GetVirtualStatus";
        private const string SetVirtualStatusProcedure = "SetVirtualStatus";

        // the last status that the user selected, this is used to set their status on login to avoid notifications to friends if their last status would have prevented such
        private const string GetLastVirtualStatusProcedure = "GetLastVirtualStatus";
        private const string SetLastVirtualStatusProcedure = "SetLastVirtualStatus";

        public StatusHandler(ISqlDataAccess _db, ILogger<StatusHandler> _logger)
        {
            db = _db;
            logger = _logger;
            db.ConnectionStringName = ConnectionStringName;
        }

        public Task<(bool result, OnlineStatus status)> TryGetStatus(string Id) => TryGet(Id, GetStatusProcedure);

        public Task<bool> SetStatus(string Id, OnlineStatus Status) => Set(Id, Status, SetStatusProcedure);

        public Task<(bool result, OnlineStatus status)> TryGetVirtualStatus(string Id) => TryGet(Id, GetVirtualStatusProcedure);

        public Task<bool> SetVirtualStatus(string Id, OnlineStatus Status) => Set(Id, Status, SetVirtualStatusProcedure);

        public Task<(bool result, OnlineStatus status)> TryGetLastVirtualStatus(string Id) => TryGet(Id, GetLastVirtualStatusProcedure);

        public Task<bool> SetLastVirtualStatus(string Id, OnlineStatus Status) => Set(Id, Status, SetLastVirtualStatusProcedure);

        private async Task<(bool result, OnlineStatus status)> TryGet(string Id, string procedure)
        {
            // make sure the id given is an actual GUID and not sql or some thing
            if (Helpers.FullVerifyGuid(ref Id, logger) is false)
            {
                return (false, default);
            }

            OnlineStatus status = await db.ExecuteSingleProcedure<OnlineStatus, dynamic>(procedure, new { Id });


            logger.LogInformation("{ProcedureName} fetched for {Id} to {Value}", procedure, Id, status);


            return (true, status);
        }

        private async Task<bool> Set(string Id, OnlineStatus Status, string procedure)
        {
            // make sure the id given is an actual GUID and not sql or some thing
            if (Helpers.FullVerifyGuid(ref Id, logger) is false)
            {
                return false;
            }

            await db.ExecuteVoidProcedure(procedure, new { Id, Status });

            logger.LogInformation("{ProcedureName} set for {Id} to {Value}", procedure, Id, Status);

            return true;
        }
    }
}
