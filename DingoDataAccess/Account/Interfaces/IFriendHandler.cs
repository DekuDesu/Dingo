using DingoDataAccess.Models.Friends;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DingoDataAccess.Account
{
    public interface IFriendHandler
    {
        Task<IFriendModel> GetFriend(string Id);
        /// <summary>
        /// Gets a list of <see cref="IFriendModel"/> for the given Id, this is used to display friend info
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        Task<List<IFriendModel>> GetFriendList(string Id);
        Task<List<IFriendModel>> GetBlocked(string Id);
        Task<List<IFriendModel>> GetRequests(string Id);
        Task<bool> AddFriend(string Id, string IdToAdd);
        Task<bool> RemoveFriend(string Id, string IdToRemove);
        Task<bool> SendRequest(string Id, string IdToSendRequestTo);
        Task<bool> RemoveRequest(string Id, string IdToRemove);
        Task<bool> BlockFriend(string Id, string IdToBlock);
        Task<bool> UnblockFriend(string Id, string IdToUnBlock);
    }
}