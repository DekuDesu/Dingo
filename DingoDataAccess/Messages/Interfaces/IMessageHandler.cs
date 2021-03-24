using DingoDataAccess.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DingoDataAccess
{
    public interface IMessageHandler
    {
        /// <summary>
        /// Gets the messages for Id from <paramref name="IdToRetrieve"/> and removes the messages after retrieval. These messages are collected by the garbage collector and never stored.
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="IdToRetrieve"></param>
        /// <returns></returns>
        Task<List<IMessageModel>> GetMessages(string Id, string IdToRetrieve);

        /// <summary>
        /// Gets the messages for the Id, does not modify the contents
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        Task<List<IMessageModel>> GetMessages(string Id);
        Task<bool> SendMessage(string SenderId, string RecipientId, IMessageModel Message);
    }
}