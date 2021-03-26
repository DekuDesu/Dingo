using System.Threading.Tasks;

namespace DingoDataAccess
{
    public interface IBundleProcessor
    {
        Task<bool> CreateSecretAndSendBundle(string SenderId, string RecipientId);
        Task<bool> SendBundle(string SenderId, string RecipientId);
        Task<bool> CreateSecret(string Id, string OtherId);
    }
}