using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DingoDataAccess
{
    public static partial class Helpers
    {
        public static bool TrySeperateDisplayName(ref string DisplayName, out (string DisplayName, short UniqueIdentifier) result)
        {
            // proper format is: string#0000

            // if the name is less than 6 chars or ( a#0000 ) then it's defacto invalid
            if (DisplayName?.Length is null or 0 or < 6 || DisplayName.Contains('#') is false)
            {
                result = default;
                return false;
            }

            // why not .Split()?
            // I wanted users to be able to have # in their name ex. #BLM#1234

            // get
            // ↓↓↓↓↓
            // string#0000
            result.DisplayName = DisplayName[..^5];

            // get
            //       ↓↓↓↓↓
            // string#0000
            string unparsedId = DisplayName[^5..];

            if (unparsedId?.Length is null or 0 or < 4 || unparsedId.Contains('#') is false)
            {
                result = default;
                return false;
            }

            // get
            //  ↓↓↓↓
            // #0000
            unparsedId = unparsedId[^4..];

            if (short.TryParse(unparsedId, out short UniqueId))
            {
                result.UniqueIdentifier = UniqueId;
                return true;
            }

            result = default;
            return false;
        }
    }
}
