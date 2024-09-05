using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{

    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        public CacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public T Get<T>(string key)
        {
            _memoryCache.TryGetValue(key, out T value);
            return value;
        }

        public void Set<T>(string key, T value, TimeSpan? expiration = null)
        {
            var options = new MemoryCacheEntryOptions();
            if (expiration.HasValue)
            {
                options.SetAbsoluteExpiration(expiration.Value);
            }
            _memoryCache.Set(key, value, options);
        }

        public void Remove(string key)
        {
            _memoryCache.Remove(key);
        }

        public bool TryGetValue<T>(string key, out T? item)
        {
            return _memoryCache.TryGetValue(key, out item);
        }

        public void Set<T>(string key, T value, MemoryCacheEntryOptions entryOptions)
        {
            _memoryCache.Set(key, value, entryOptions);
        }
    }


    public interface ICacheService
    {
        T Get<T>(string key);
        bool TryGetValue<T>(string key, out T? item);
        void Set<T>(string key, T value, TimeSpan? expiration = null);
        void Set<T>(string key, T value, MemoryCacheEntryOptions entryOptions);
        void Remove(string key);
    }
}
