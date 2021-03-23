using System.Threading.Tasks;

namespace DingoDataAccess
{
    public interface IEncryptedClientStateHandler
    {
        Task<string> GetState(string Id);
        Task<bool> SetState(string Id, string State);
    }
}