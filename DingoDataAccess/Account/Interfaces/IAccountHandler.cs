using System.Threading.Tasks;

namespace DingoDataAccess.Account
{
    public interface IAccountHandler
    {
        Task<bool> CreateNewAccount(string Id, string DisplayName);
        Task<bool> DeleteAccount(string Id);
    }
}