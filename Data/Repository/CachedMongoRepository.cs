using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Threading;
using TelegramQueueBot.Data.Abstraction;
using TelegramQueueBot.Models;

namespace Data.Repository
{
    public class CachedMongoRepository<TRepository, TEntity> : IRepository<TEntity> 
        where TEntity : Entity, new()
        where TRepository : IRepository<TEntity>
    {
        protected TRepository _innerRepository;
        protected readonly ILogger _log;
        protected IMemoryCache _cache;
        public CachedMongoRepository(TRepository innerRepository, ILogger log, IMemoryCache cache)
        {
            _innerRepository = innerRepository;
            _log = log;
            _cache = cache;
        }

        public virtual async Task<TEntity> GetAsync(string id)
        {
            if (_cache.TryGetValue(id, out TEntity cachedItem))
            {
                _log.LogDebug($"Entity with Id {id} retrieved from cache.");
                return cachedItem;
            }

            var item = await _innerRepository.GetAsync(id);
            if (item != null)
            {
                _cache.Set(id, item);
                _log.LogDebug($"Entity with Id {id} added to cache.");
            }

            return item;
        }

        public virtual async Task<TEntity> CreateAsync(TEntity item)
        {
            var createdItem = await _innerRepository.CreateAsync(item);
            _cache.Set(createdItem.Id, createdItem);
            _log.LogDebug($"Entity with Id {createdItem.Id} added to cache.");
            return createdItem;
        }

        public virtual async Task<bool> UpdateAsync(TEntity item)
        {
            var result = await _innerRepository.UpdateAsync(item);
            if (result)
            {
                _cache.Set(item.Id, item);
                _log.LogDebug($"Entity with Id {item.Id} updated in cache.");
            }
            return result;
        }

        public virtual async Task<bool> DeleteAsync(string id)
        {
            var result = await _innerRepository.DeleteAsync(id);
            if (result)
            {
                _cache.Remove(id);
                _log.LogInformation($"Entity with Id {id} removed from cache.");
            }
            return result;
        }

    }
}
