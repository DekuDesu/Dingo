using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DingoDataAccess
{
    public static partial class Helpers
    {
        /// <summary>
        /// Does a VERY basic removal of common sql escape characters, this should be used in conjunciton Stored Procedures to reduce the chance
        /// of sql injection attacks.
        /// </summary>
        /// <param name="possibleSQL"></param>
        /// <returns></returns>
        public static void CleanInputBasic(ref string possibleSQL)
        {
            possibleSQL = possibleSQL.Replace("); DROP TABLE STUDENTS; --", "xkcd#327");
            possibleSQL = possibleSQL.Replace("DROP TABLE", "xkcd#327");
            possibleSQL = possibleSQL.Replace("SELECT *", "xkcd#327");
            possibleSQL = possibleSQL.Replace('\'', ' ');
            possibleSQL = possibleSQL.Replace(';', ' ');
            possibleSQL = possibleSQL.Replace('=', ' ');
        }
    }
}
