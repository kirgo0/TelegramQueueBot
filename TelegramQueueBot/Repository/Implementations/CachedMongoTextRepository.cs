using Data.Repository;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;

namespace TelegramQueueBot.Repository.Implementations
{
    public class CachedMongoTextRepository : CachedMongoRepository<MongoTextRepository, Text>, ITextRepository
    {
        public CachedMongoTextRepository(MongoTextRepository innerRepository, ILogger<CachedMongoTextRepository> log, IMemoryCache cache) : base(innerRepository, log, cache)
        {
        }

        public async Task<Text> GetByKeyAsync(string key)
        {
            try
            {
                if (_cache.TryGetValue(key, out Text cachedItem))
                {
                    _log.LogDebug("Text with Id {id} retrieved from cache", cachedItem.Id);
                    return cachedItem;
                }

                var item = await _innerRepository.GetByKeyAsync(key);
                if (item != null)
                {
                    _cache.Set(key, item);
                    _log.LogDebug("Text with Id {id} added to cache", item.Id);
                }

                return item;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred when retrieving a text for {key}.", key);
                return null;
            }
        }
    }
}
