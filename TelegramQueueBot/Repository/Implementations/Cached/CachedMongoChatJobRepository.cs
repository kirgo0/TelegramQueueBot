using Data.Repository;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;

namespace TelegramQueueBot.Repository.Implementations.Cached
{
    public class CachedChatMongoJobRepository : CachedMongoRepository<MongoChatJobRepository, ChatJob>, IChatJobRepository
    {
        public CachedChatMongoJobRepository(MongoChatJobRepository innerRepository, ILogger<CachedChatMongoJobRepository> log, IMemoryCache cache, MemoryCacheEntryOptions cacheOptions) : base(innerRepository, log, cache, cacheOptions)
        {
        }

        private string GetChatKey(long chatId)
        {
            return $"ChatJobsList_{chatId}";
        }
        public async Task<List<ChatJob>> GetAllByChatIdAsync(long chatId)
        {
            if (_cache.TryGetValue(GetChatKey(chatId), out List<string> chatJobsIds))
            {
                _log.LogDebug("Chat {id} job ids list retrived from cache", chatId);

                var result = new List<ChatJob>();
                var missedJobIds = new List<string>();
                for (var i = 0; i < chatJobsIds.Count; i++)
                {
                    var chatJobId = chatJobsIds[i];
                    if (_cache.TryGetValue(GetKey(chatJobId), out ChatJob? chatJob)) {
                        result.Add(chatJob);
                    } else
                    {
                        missedJobIds.Add(chatJobId);
                    }
                }

                if (missedJobIds.Count > 0)
                {
                    var dbResult = await _innerRepository.GetAllByIdsAsync(missedJobIds);
                    AddOrUpdateCacheRange(dbResult);
                    result.AddRange(dbResult);
                    _log.LogDebug("The missing chat {chatId} jobs {values} were retrived from database", chatId, string.Join(',', chatJobsIds));
                } else if(result.Count > 0)
                {
                    _log.LogDebug("All chat {chatId} jobs were retrived from cache", chatId);
                } else
                {
                    _log.LogDebug("No chat {chatId} jobs were retrived from database", chatId);
                }
                return result;
            } else
            {
                var result = await _innerRepository.GetAllByChatIdAsync(chatId);
                if(AddOrUpdateCacheRange(result))
                    _log.LogDebug("All chat {chatId} missing jobs were added to cache", chatId);
                return result;
            }

        }

        protected override void AddOrUpdateCache(ChatJob item)
        {
            _cache.Set(GetKey(item.Id), item, _cacheOptions);
            if(_cache.TryGetValue(GetChatKey(item.ChatId), out List<string> chatJobIds)) {
                if (!chatJobIds.Contains(item.Id))
                {
                    chatJobIds.Add(item.Id);
                    _cache.Set(GetChatKey(item.ChatId), chatJobIds, _cacheOptions);
                }
            }
        }
        
        protected bool AddOrUpdateCacheRange(List<ChatJob> items)
        {
            if (items.Count == 0) return false;
            foreach(var item in items)
            {
                _cache.Set(GetKey(item.Id), item, _cacheOptions);
            }
            if (_cache.TryGetValue(GetChatKey(items[0].ChatId), out List<string> chatJobIds))
            {
                _cache.Set(GetChatKey(items[0].ChatId), chatJobIds.Union(items.Select(x => x.Id)).ToList(), _cacheOptions);
            } else
            {
                _cache.Set(GetChatKey(items[0].ChatId), items.Select(x => x.Id).ToList(), _cacheOptions);
            }
            return true;
        }

        protected override void RemoveFromCache(ChatJob item)
        {
            _cache.Remove(GetKey(item.Id));
            if (_cache.TryGetValue(GetChatKey(item.ChatId), out List<string> chatJobIds))
            {
                if (chatJobIds.Contains(item.Id))
                {
                    chatJobIds.Remove(item.Id);
                    _cache.Set(GetChatKey(item.ChatId), chatJobIds, _cacheOptions);
                }
            }
        }

    }
}
