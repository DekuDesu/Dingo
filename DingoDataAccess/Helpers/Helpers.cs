using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DingoDataAccess
{
    public static partial class Helpers
    {
        /// <summary>
        /// Verifies that a string parameter is a valid Gui, is not null or empty, and logs a warning if it's is a possble sql injection, and cleans any sql characters if contained
        /// </summary>
        /// <returns></returns>
        public static bool FullVerifyGuid(ref string GuidToCheck, ILogger Logger, [CallerMemberName] string caller = "", [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string filePath = "")
        {
            if (GuidToCheck?.Length is null or 0)
            {
                Logger.LogInformation("Failed to verify Guid {Guid} at {Caller}{LineNumber} - {FilePath}", GuidToCheck, caller, lineNumber, filePath);
                return false;
            }

            // clean sql characters from the input to reduce the chance of sql injections
            Helpers.CleanInputBasic(ref GuidToCheck);

            if (Guid.TryParse(GuidToCheck, out Guid _) is false)
            {
                Logger.LogWarning("Failed to verify Guid, possible SQL injection {Guid} at {Caller}{LineNumber} - {FilePath}", GuidToCheck, caller, lineNumber, filePath);
                return false;
            }
            return true;
        }
    }
}
