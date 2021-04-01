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

        private readonly Dictionary<string, System.Timers.Timer> Timers = new();

        private readonly SemaphoreSlim DictionaryLimiter = new(1, 1);

        public bool VerboseLogging { get; set; } = false;

        public T logger { get; set; }

        private readonly SemaphoreSlim TimerLimiter = new(1, 1);

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
            var timer = new System.Timers.Timer()
            {
                Interval = RefreshRate,
                AutoReset = true,
                Enabled = true
            };

            // build the event
            ElapsedEventHandler newEvent = async (x, y) =>
            {
                // since these timers like to continue on after we have disposed them so we have to try to prevent race conditions from duplicating calls for ghost users

                // lock ALL timers
                if (await TimerLimiter?.WaitAsync(5))
                {
                    Callback?.Invoke();

                    // allow other timers to invoke
                    TimerLimiter.Release();
                }
                else
                {
                    if (VerboseLogging)
                    {
                        logger?.LogInformation("Deadmans Encountered Exiting ({Id})", Key);
                    }
                    timer?.Stop();
                    timer?.Dispose();
                }
            };

            // assign the event so it can be called recurrently
            timer.Elapsed += newEvent;

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
            // force the dictionary to wait
            DictionaryLimiter?.Wait();

            // force all the timers to wait on their events
            TimerLimiter?.Wait();

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

            if (VerboseLogging)
            {
                logger?.LogInformation("Disposed Timer Dict Total: ({Total})", Timers.Count);
            }
            DictionaryLimiter.Release();
            TimerLimiter?.Release();
            // TimerLimiter intentionally not released to force timers on other threads to exit
        }
    }
}
