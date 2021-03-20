using DingoDataAccess.Models.Friends;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DingoDataAccess.Account
{
    public interface IFriendListHandler
    {
        /// <summary>
        /// Sets a list of Guids for the given id
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="FriendsList"></param>
        /// <returns></returns>
        Task<bool> SetFriendIds(string Id, List<string> FriendsList);

        /// <summary>
        /// Gets a list of Guids for the given id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        Task<List<string>> GetFriendIds(string Id);

        /// <summary>
        /// Sets a list of Guids for the given id
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="FriendsList"></param>
        /// <returns></returns>
        Task<bool> SetBlockedIds(string Id, List<string> FriendsList);

        /// <summary>
        /// Gets a list of Guids for the given id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        Task<List<string>> GetBlockedIds(string Id);

        /// <summary>
        /// Sets a list of Guids for the given id
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="FriendsList"></param>
        /// <returns></returns>
        Task<bool> SetRequestIds(string Id, List<string> FriendsList);

        /// <summary>
        /// Gets a list of Guids for the given id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        Task<List<string>> GetRequestIds(string Id);
    }
}