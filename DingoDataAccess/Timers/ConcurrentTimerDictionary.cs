using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DingoDataAccess
{
    public class ConcurrentTimerDictionary : IDisposable, IConcurrentTimerDictionary
    {

        private readonly Dictionary<string, System.Timers.Timer> Timers = new();

        private readonly SemaphoreSlim Limiter = new(1, 1);

        public Action OnTimer { get; set; }

        public async Task<string> AddTimer(int RefreshRate, Func<Task> Callback, string Key = null)
        {
            // create a new timer that will periodilcally invoke the provided action
            var timer = new System.Timers.Timer()
            {
                Interval = RefreshRate,
                AutoReset = true,
                Enabled = true
            };

            timer.Elapsed += async (x, y) => await Callback?.Invoke();

            timer.Elapsed += (x, y) => OnTimer();

            string Id = Key ?? Guid.NewGuid().ToString();

            await Limiter.WaitAsync();

            if (Timers.ContainsKey(Id) is false)
            {
                Timers.Add(Id, timer);
            }

            Limiter.Release();

            timer.Start();

            return Id;
        }

        public async Task RemoveTimer(string Id)
        {
            await Limiter.WaitAsync();

            if (Timers.ContainsKey(Id))
            {
                var timer = Timers[Id];

                timer.Stop();
                timer.Close();
                timer.Dispose();

                Timers.Remove(Id);
            }

            Limiter.Release();
        }

        public void Dispose()
        {
            Limiter.Wait();

            foreach (var item in Timers.Values)
            {
                item.Stop();
                item.Close();
                item.Dispose();
            }

            Limiter.Release();
        }
    }
}
