using System.Threading.Tasks;

namespace DingoDataAccess.Account
{
    public interface IDisplayNameHandler
    {
        Task<string> GetDisplayName(string id);
        Task<bool> SetDisplayName(string id, string username);
        Task<short?> GetAvailableUniqueIdentifier(string DisplayName);
        Task<bool> SetUniqueIdentifier(string Id, short UniqueIdentifier);
        Task<string> GetIdFromDisplayName(string DisplayName);
    }
}