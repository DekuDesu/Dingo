using System;
using System.Threading.Tasks;

namespace Dingo
{
    public interface ITopLevelObjects
    {
        string CurrentChatId { get; set; }
        Action StateHasChanged { get; set; }

        Task<string> AddTimer(int RefreshRate, Func<Task> Callback, string Key = null);
        void Dispose();
        Task RemoveTimer(string Key);
    }
}