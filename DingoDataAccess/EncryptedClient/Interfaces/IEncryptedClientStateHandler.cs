using System.Threading.Tasks;

namespace DingoDataAccess
{
    public interface IEncryptedClientStateHandler
    {
        /// <summary>
        /// Gets the state that can be imported to communicate with FriendId
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="FriendId"></param>
        /// <returns></returns>
        Task<string> GetState(string Id, string FriendId);

        /// <summary>
        /// Sets the state for future use to communicate with the FriendId
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="FriendId"></param>
        /// <param name="State"></param>
        /// <returns></returns>
        Task<bool> SetState(string Id, string FriendId, string State);

        /// <summary>
        /// Deletes a state if its found in the users state dictionary. Returns true when no error encountered. False when either the params given are missing, corrupted, or contain possible SQL, or when error encoutered executing qwuery.
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="FriendId"></param>
        /// <returns></returns>
        Task<bool> DeleteState(string Id, string FriendId);
    }
}