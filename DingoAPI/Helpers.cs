using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DingoAPI
{
    public static class Helpers
    {
        public static Task Wait(int ms)
        {
            return Task.Run(() => Thread.Sleep(ms));
        }
    }
}
