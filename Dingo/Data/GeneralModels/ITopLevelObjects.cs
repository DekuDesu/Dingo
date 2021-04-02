using System;
using System.Threading.Tasks;

namespace Dingo
{
    public interface ITopLevelObjects : IDisposable
    {
        string CurrentChatId { get; set; }
        Action StateHasChanged { get; set; }
    }
}