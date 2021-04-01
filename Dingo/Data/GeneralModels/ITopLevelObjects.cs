using System;
using System.Threading.Tasks;

namespace Dingo
{
    public interface ITopLevelObjects : IDisposable
    {
        string CurrentChatId { get; set; }
        Action StateHasChanged { get; set; }

        Task<string> AddTimer(int RefreshRate, Func<Task> Callback, string Key = null);
        Task RemoveTimer(string Key);
    }
}