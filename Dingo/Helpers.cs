using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dingo
{
    public static class Helpers
    {
        public static Task Sleep(int ms = 100)
        {
            return Task.Run(() => System.Threading.Thread.Sleep(ms));
        }
    }
}
