using System.Threading.Tasks;

namespace DingoDataAccess
{
    public interface IQueryCache<T, U>
    {
        int MaxItemsInCache { get; set; }

        Task UpdateOrCache(string query, U parameters, T itemToCache);
        Task<T> GetCachedOrDefault(string query, U parameters);
    }
}