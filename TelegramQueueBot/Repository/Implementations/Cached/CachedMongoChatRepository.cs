using Data.Repository;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;

namespace TelegramQueueBot.Repository.Implementations.Cached
{
    public class CachedMongoChatRepository : CachedMongoRepository<MongoChatRepository, Chat>, IChatRepository
    {
        public CachedMongoChatRepository(MongoChatRepository innerRepository, ILogger log, IMemoryCache cache, MemoryCacheEntryOptions cacheOptions) : base(innerRepository, log, cache, cacheOptions)
        {
        }

        public async Task<Chat> GetByTelegramIdAsync(long telegramId)
        {
            try
            {
                if (_cache.TryGetValue(GetKey(telegramId), out Chat cachedItem))
                {
                    _log.LogDebug("Chat with TelegramId {telegramId} retrieved from cache", telegramId);
                    return cachedItem;
                }

                var item = await _innerRepository.GetByTelegramIdAsync(telegramId);
                if (item != null)
                {
                    AddOrUpdateCache(item);
                    _log.LogDebug("Chat with TelegramId {telegramId} added to cache", telegramId);
                }

                return item;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred when retrieving a chat with Telegram ID {id}.", telegramId);
                return null;
            }
        }

        protected override void AddOrUpdateCache(Chat item)
        {
            _cache.Set(GetKey(item.TelegramId), item, _cacheOptions);
            _cache.Set(GetKey(item.Id), item, _cacheOptions);
        }

        protected override void RemoveFromCache(Chat item)
        {
            _cache.Remove(GetKey(item.TelegramId));
            _cache.Remove(GetKey(item.Id));
        }
    }
}
