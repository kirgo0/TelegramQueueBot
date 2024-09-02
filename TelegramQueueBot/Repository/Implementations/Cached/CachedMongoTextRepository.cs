using Data.Repository;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Reflection;
using TelegramQueueBot.Common;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;

namespace TelegramQueueBot.Repository.Implementations.Cached
{
    public class CachedMongoTextRepository : CachedMongoRepository<MongoTextRepository, Text>, ITextRepository
    {
        public CachedMongoTextRepository(MongoTextRepository innerRepository, ILogger log, IMemoryCache cache, MemoryCacheEntryOptions cacheOptions = null) : base(innerRepository, log, cache, cacheOptions)
        {
        }

        public async Task<Text> GetByKeyAsync(string key)
        {
            try
            {
                if (_cache.TryGetValue(GetKey(key), out Text cachedItem))
                {
                    _log.LogDebug("Text with Key {id} retrieved from cache", cachedItem.Key);
                    return cachedItem;
                }

                var item = await _innerRepository.GetByKeyAsync(key);
                if (item != null && !item.Equals(MongoTextRepository.NotFoundText))
                {
                    _cache.Set(GetKey(item.Key), item, _cacheOptions);
                    _log.LogDebug("Text with Key {id} added to cache", item.Key);
                }

                return item;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred when retrieving a text with key {key}.", key);
                return null;
            }
        }

        public async Task<string> GetValueAsync(string key)
        {
            var text = await GetByKeyAsync(key);
            return text is not null ? text.Value : string.Empty;
        }

    }
}
