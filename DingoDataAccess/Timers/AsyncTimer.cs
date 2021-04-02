using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace DingoDataAccess.Timers
{
    public class AsyncTimer : IDisposable, IAsyncTimer
    {
        private readonly System.Timers.Timer Timer = new() { Enabled = true };

        /// <summary>
        /// This is to ensure only one lock can perform operations at a time, if this object is not set this timer will continue to invoke on background threads after it's been disposed
        /// </summary>
        public object TimerLock { get; set; } = new();

        /// <summary>
        /// This ensures that once this lock as been set that the timer does not invoke any callbacks that may affect things after it's been disposed. If this is not set further invocations may happen on background threads
        /// </summary>
        public ManualResetEvent DeadmansLock { get; set; } = new(true);

        public double Interval
        {
            get { return Timer.Interval; }
            set { Timer.Interval = value; }
        }

        public bool AutoReset
        {
            get { return Timer.AutoReset; }
            set { Timer.AutoReset = value; }
        }

        public void Elapsed(Action onElapsed)
        {
            lock (TimerLock)
            {
                Timer.Elapsed += CreateElapsedEvent(onElapsed);
            }
        }

        private ElapsedEventHandler CreateElapsedEvent(Action onElapsed)
        {
            return (x, y) =>
            {
                lock (TimerLock)
                {
                    if (DeadmansLock.WaitOne(0))
                    {
                        return;
                    }
                    onElapsed?.Invoke();
                }
            };

        }

        public void Start()
        {
            lock (TimerLock)
            {
                DeadmansLock.Reset();
                Timer?.Start();
            }
        }

        public void Stop()
        {
            lock (TimerLock)
            {
                DeadmansLock.Set();
                Timer?.Stop();
            }
        }

        public void Dispose()
        {
            lock (TimerLock)
            {
                // Prevent any running events from actually invoking anything
                DeadmansLock.Set();

                // stop and dispose the timer
                Timer?.Stop();
                Timer?.Dispose();
            }
        }
    }
}
