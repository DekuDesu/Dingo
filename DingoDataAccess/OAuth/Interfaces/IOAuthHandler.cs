using System.Threading.Tasks;

namespace DingoDataAccess.OAuth
{
    public interface IOAuthHandler
    {
        Task<string> GetOAuth(string Id);
        Task<bool> SetOAuth(string Id, string OAuth);
        Task<bool> UpdateOAuth(string Id, string OAuth);
    }
}