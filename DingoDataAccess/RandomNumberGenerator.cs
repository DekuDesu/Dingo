using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DingoDataAccess
{
    public class RandomNumberGenerator
    {
        private static Random Generator = new();

        private static SemaphoreSlim locker = new(1, 1);

        /// <summary>
        /// Returns a random number between min(inclusive) and max(exclusive)
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static async Task<int> Next(int min, int max)
        {
            await locker.WaitAsync();
            try
            {
                return Generator.Next(min, max);
            }
            finally
            {
                locker.Release();
            }
        }

        public static async Task<int> NextUnique(IEnumerable<int> numbers, int min, int max)
        {
            await locker.WaitAsync();
            try
            {
                int result = 0;
                void RollForUnique()
                {
                    do
                    {
                        result = Generator.Next(min, max);
                    } while (numbers.Contains(result));
                }
                await Task.Run(RollForUnique);
                return result;
            }
            finally
            {
                locker.Release();
            }
        }
        public static async Task<short> NextUnique(IEnumerable<short> numbers, short min, short max)
        {
            await locker.WaitAsync();
            try
            {
                int result = 0;
                void RollForUnique()
                {
                    do
                    {
                        result = Generator.Next(min, max);
                    } while (numbers.Contains((short)result));
                }
                await Task.Run(RollForUnique);
                return (short)result;
            }
            finally
            {
                locker.Release();
            }
        }
    }
}
