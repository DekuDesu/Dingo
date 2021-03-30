using System.Threading.Tasks;

namespace DingoDataAccess
{
    public interface IAvatarHandler
    {
        Task<string> GetAvatar(string Id);
        Task<bool> SetAvatar(string Id, string Base64Avatar);
    }
}