using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DingoDataAccess
{
    public class QueryCache<T, U> : IQueryCache<T, U>
    {
        private HashSet<(System.Timers.Timer, (string, U))> ExpirationTimers = new();

        public int DefaultExpirationTime = 30000;

        public QueryCache(ILogger<QueryCache<T, U>> logger)
        {
            this.logger = logger;
        }

        private Dictionary<(string, U), T> Dict { get; set; } = new();

        private readonly SemaphoreSlim semaphore = new(1, 1);
        private readonly ILogger<QueryCache<T, U>> logger;

        public int MaxItemsInCache { get; set; } = 1000;

        public bool Contains(string query, U parameter) => Dict.ContainsKey((query, parameter));

        public async Task<T> GetCachedOrDefault(string query, U parameters)
        {
            var key = (query, parameters);

            await semaphore.WaitAsync();

            try
            {
                if (Dict.ContainsKey(key))
                {
                    // make sure the dict isnt gigantic
                    VerifyCache();

                    T item = Dict[key];

                    logger.LogInformation("Found cached item for {Query} Item: {Item}", key, item);

                    return item;
                }
            }
            finally
            {
                semaphore.Release();
            }

            return default;
        }

        public async Task UpdateOrCache(string query, U parameters, T itemToCache)
        {
            var key = (query, parameters);

            await semaphore.WaitAsync();

            try
            {
                if (Dict.ContainsKey(key))
                {
                    Dict[key] = itemToCache;
                    return;
                }
                // make sure the dict isnt gigantic
                VerifyCache();

                Dict.Add((query, parameters), itemToCache);

                System.Timers.Timer newExpirationTimer = new()
                {
                    Interval = DefaultExpirationTime,
                    Enabled = true,
                };

                newExpirationTimer.Elapsed += async (x, y) =>
                {
                    await DecacheKey(key);
                    if (ExpirationTimers.Contains((newExpirationTimer, key)))
                    {
                        ExpirationTimers.Remove((newExpirationTimer, key));
                    }
                    newExpirationTimer?.Close();
                    newExpirationTimer?.Dispose();
                };

                newExpirationTimer.Start();

                ExpirationTimers.Add((newExpirationTimer, (query, parameters)));

                logger.LogInformation("Cached item for {Query} Item: {Item}", key, itemToCache);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task DeCacheItem(T itemToDeCache)
        {
            if (Dict.Values.Contains(itemToDeCache))
            {
                var key = (from entry in Dict where entry.Value.Equals(itemToDeCache) select entry.Key).FirstOrDefault();

                await semaphore.WaitAsync();

                try
                {
                    Dict.Remove(key);

                    VerifyCache();
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        private async Task DecacheKey((string, U) key)
        {
            if (Dict.ContainsKey(key))
            {
                await semaphore.WaitAsync();

                try
                {
                    Dict.Remove(key);

                    VerifyCache();
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        private void VerifyCache()
        {
            if (Dict.Count > MaxItemsInCache)
            {
                Dict.Clear();
            }
        }
    }
}
