using DingoDataAccess.Enums;
using System.Threading.Tasks;

namespace DingoDataAccess.Account
{
    public interface IStatusHandler
    {
        /// <summary>
        /// Sets the last virtual status for a user, this is the status the user will be set to when they log in
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="Status"></param>
        /// <returns></returns>
        Task<bool> SetLastVirtualStatus(string Id, OnlineStatus Status);

        /// <summary>
        /// Sets the actual status of the user, it should either be Online or Offline
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="Status"></param>
        /// <returns></returns>
        Task<bool> SetStatus(string Id, OnlineStatus Status);

        /// <summary>
        /// sets the virtual status of the user, this is the status the user chooses to display to other users, should be anything BUT Offline as that is reserved for actual offline status
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="Status"></param>
        /// <returns></returns>
        Task<bool> SetVirtualStatus(string Id, OnlineStatus Status);

        /// <summary>
        /// Gets the last virtual status of the user, this was the last status they they were before the logged out
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        Task<(bool result, OnlineStatus status)> TryGetLastVirtualStatus(string Id);

        /// <summary>
        /// Gets the last actual virtual status of the user, this is either Online or Offline
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        Task<(bool result, OnlineStatus status)> TryGetStatus(string Id);

        /// <summary>
        /// Gets the current virtual status of the user, this is what the user has selected to display to other users
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        Task<(bool result, OnlineStatus status)> TryGetVirtualStatus(string Id);
    }
}