using DingoDataAccess.Timers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace DingoDataAccess
{
    public sealed class ConcurrentTimerDictionary<T> : IDisposable, IConcurrentTimerDictionary where T : ILogger
    {

        private readonly Dictionary<string, AsyncTimer> Timers = new();

        private readonly SemaphoreSlim DictionaryLimiter = new(1, 1);

        public object LockingObject { get; set; } = new object();

        public ManualResetEvent DeadmansLock { get; set; } = new(false);

        public bool VerboseLogging { get; set; } = false;

        public T logger { get; set; }

        private bool disposed = false;

        public ConcurrentTimerDictionary(T _logger)
        {
            logger = _logger;
            if (VerboseLogging)
            {
                logger?.LogInformation("Created {ObjectName}", nameof(ConcurrentTimerDictionary<T>));
            }
        }

        public ConcurrentTimerDictionary()
        {

        }

        public Action OnTimer { get; set; }

        public async Task<string> AddTimer(int RefreshRate, Func<Task> Callback, string Key = null)
        {
            // create a new timer that will periodilcally invoke the provided action
            var timer = new AsyncTimer()
            {
                Interval = RefreshRate,
                AutoReset = false,
                DeadmansLock = DeadmansLock,
                TimerLock = LockingObject
            };

            // build the event
            timer.Elapsed(() =>
            {
                Callback();
                if (!disposed)
                {
                    timer.Start();
                }
            });

            string Id = Key ?? Guid.NewGuid().ToString();

            // add the timer to the dict

            // wait for dict to be free
            await DictionaryLimiter.WaitAsync();

            if (Timers.ContainsKey(Id) is false)
            {
                Timers.Add(Id, timer);
            }

            // free the dict so others can use it
            DictionaryLimiter.Release();

            // start the timer
            timer.Start();

            if (VerboseLogging)
            {
                logger?.LogInformation("Timer Started ({Id}) Total: ({Total})", Id, Timers.Count);
            }

            return Id;
        }

        public async Task RemoveTimer(string Id)
        {
            await DictionaryLimiter.WaitAsync();

            if (Timers.ContainsKey(Id))
            {
                var timer = Timers[Id];

                timer?.Stop();
                timer?.Dispose();

                Timers?.Remove(Id);
            }

            DictionaryLimiter.Release();

            if (VerboseLogging)
            {
                logger?.LogInformation("Timer Removed ({Id}) Total: ({Total})", Id, Timers.Count);
            }
        }

        public void Dispose()
        {
            disposed = true;
            // force the dictionary to wait
            DictionaryLimiter?.Wait();

            foreach (var item in Timers)
            {
                if (VerboseLogging)
                {
                    logger?.LogInformation("Timer Disposed ({Id}) Total: ({Total})", item.Key, Timers.Count);
                }

                item.Value.Stop();
                item.Value.Dispose();
            }

            Timers.Clear();

            DictionaryLimiter.Release();
        }
    }
}
