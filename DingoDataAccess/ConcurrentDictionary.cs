using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DingoDataAccess
{
    public class ConcurrentDictionary<T, U> : IEnumerable<KeyValuePair<T, U>>
    {
        private readonly Dictionary<T, U> Dict = new();

        private readonly SemaphoreSlim Limiter = new(1, 1);

        public int Count => Dict.Count;

        public U this[T Key]
        {
            get
            {
                Limiter.Wait();

                try
                {
                    return Dict[Key];
                }
                finally
                {
                    Limiter.Release();
                }
            }
            set
            {
                Add(Key, value).Wait();
            }
        }

        public async Task Add(T Key, U Value)
        {
            await Limiter.WaitAsync();
            try
            {
                if (Dict.ContainsKey(Key))
                {
                    Dict[Key] = Value;
                }
                else
                {
                    Dict.Add(Key, Value);
                }
            }
            finally
            {
                Limiter.Release();
            }
        }

        public async Task Remove(T Key)
        {
            await Limiter.WaitAsync();
            try
            {
                if (Dict.ContainsKey(Key) is false)
                {
                    Dict.Remove(Key);
                }
            }
            finally
            {
                Limiter.Release();
            }
        }

        public async Task<bool> Contains(T Key)
        {
            await Limiter.WaitAsync();
            try
            {
                return Dict.ContainsKey(Key);
            }
            finally
            {
                Limiter.Release();
            }
        }

        public async Task Clear()
        {
            await Limiter.WaitAsync();
            try
            {
                Dict.Clear();
            }
            finally
            {
                Limiter.Release();
            }
        }

        public IEnumerator<KeyValuePair<T, U>> GetEnumerator() => Dict.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Dict.GetEnumerator();
    }
}
