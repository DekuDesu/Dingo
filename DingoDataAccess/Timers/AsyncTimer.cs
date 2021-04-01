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
    public class AsyncTimer : IDisposable
    {
        private readonly System.Timers.Timer Timer = new();
        private readonly ILogger<AsyncTimer> logger;

        private readonly object TimerLock = new();
        private readonly ManualResetEvent DeadmansLock = new(false);

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

        public AsyncTimer(ILogger<AsyncTimer> _logger)
        {
            logger = _logger;
        }

        public void Elapsed(ElapsedEventHandler newEvent)
        {
            Timer.Elapsed += newEvent;
        }

        public void Start()
        {
            lock (TimerLock)
            {
                Timer?.Start();
            }
        }

        public void Stop()
        {
            lock (TimerLock)
            {
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
