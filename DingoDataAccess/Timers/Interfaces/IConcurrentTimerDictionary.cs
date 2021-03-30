using System;
using System.Threading.Tasks;

namespace DingoDataAccess
{
    public interface IConcurrentTimerDictionary
    {
        Action OnTimer { get; set; }

        Task<string> AddTimer(int RefreshRate, Func<Task> Callback, string Key = null);
        void Dispose();
        Task RemoveTimer(string Id);
    }
}