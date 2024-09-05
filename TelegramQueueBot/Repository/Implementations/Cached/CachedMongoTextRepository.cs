using Data.Repository;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;

namespace TelegramQueueBot.Repository.Implementations.Cached
{
    public class CachedMongoTextRepository : CachedMongoRepository<MongoTextRepository, Text>, ITextRepository
    {
        public CachedMongoTextRepository(MongoTextRepository innerRepository, ILogger<CachedMongoTextRepository> log, IMemoryCache cache, MemoryCacheEntryOptions cacheOptions) : base(innerRepository, log, cache, cacheOptions)
        {
        }

        public async Task<Text> GetByKeyAsync(string key)
        {
            try
            {
                if (_cache.TryGetValue(GetKey(key), out Text? cachedItem))
                {
                    _log.LogDebug("Text with Key {id} retrieved from cache", cachedItem.Key);
                    return cachedItem;
                }

                var item = await _innerRepository.GetByKeyAsync(key);
                if (item != null && !item.Equals(MongoTextRepository.NotFoundText))
                {
                    AddOrUpdateCache(item);
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

        protected override void AddOrUpdateCache(Text item)
        {
            _cache.Set(GetKey(item.Key), item, _cacheOptions);
        }

        protected override void RemoveFromCache(Text item)
        {
            _cache.Remove(GetKey(item.Key));
        }
    }
}
