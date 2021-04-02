using System;
using System.Threading;

namespace DingoDataAccess.Timers
{
    public interface IAsyncTimer
    {
        bool AutoReset { get; set; }
        ManualResetEvent DeadmansLock { get; set; }
        double Interval { get; set; }
        object TimerLock { get; set; }

        void Dispose();
        void Elapsed(Action onElapsed);
        void Start();
        void Stop();
    }
}