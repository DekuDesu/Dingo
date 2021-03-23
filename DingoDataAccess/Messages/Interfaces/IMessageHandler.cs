using DingoDataAccess.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DingoDataAccess
{
    public interface IMessageHandler
    {
        Task<List<IMessageModel>> GetMessages(string Id);
        Task<bool> SendMessage(string SenderId, string RecipientId, IMessageModel Message);
    }
}