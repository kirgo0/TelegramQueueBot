using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TelegramQueueBot.Data.Abstraction;
using TelegramQueueBot.Models;

namespace Data.Repository
{
    public class CachedMongoRepository<TRepository, TEntity> : IRepository<TEntity>
        where TEntity : Entity, new()
        where TRepository : IRepository<TEntity>
    {
        protected readonly TRepository _innerRepository;
        protected readonly ILogger _log;
        protected readonly IMemoryCache _cache;
        protected readonly string _cacheKeyPrefix;
        protected readonly MemoryCacheEntryOptions _cacheOptions;

        public CachedMongoRepository(TRepository innerRepository, ILogger log, IMemoryCache cache, MemoryCacheEntryOptions cacheOptions = null)
        {
            _innerRepository = innerRepository;
            _log = log;
            _cache = cache;
            _cacheKeyPrefix = typeof(TEntity).Name;
            _cacheOptions = cacheOptions ?? new MemoryCacheEntryOptions(); 
        }

        public virtual async Task<TEntity> GetAsync(string id)
        {
            if (_cache.TryGetValue(GetKey(id), out TEntity cachedItem))
            {
                _log.LogDebug("{name} with Id {id} retrieved from cache", typeof(TEntity).Name, id);
                return cachedItem;
            }

            var item = await _innerRepository.GetAsync(id);
            if (item != null)
            {
                AddOrUpdateCache(item);
                _log.LogDebug("{name} with Id {id} added to cache", typeof(TEntity).Name, id);
            }

            return item;
        }

        public virtual async Task<TEntity> CreateAsync(TEntity item)
        {
            var createdItem = await _innerRepository.CreateAsync(item);
            AddOrUpdateCache(createdItem);
            _log.LogDebug("{name} with Id {id} added to cache", typeof(TEntity).Name, createdItem.Id);
            return createdItem;
        }

        public virtual async Task<bool> UpdateAsync(TEntity item)
        {
            var result = await _innerRepository.UpdateAsync(item);
            if (result)
            {
                AddOrUpdateCache(item);
                _log.LogDebug("{name} with Id {id} updated in cache", typeof(TEntity).Name, item.Id);
            }
            return result;
        }

        public virtual async Task<bool> DeleteAsync(string id)
        {
            var item = await _innerRepository.GetAsync(id);
            if (item is null) return false;
            var result = await _innerRepository.DeleteAsync(id);
            if (result)
            {
                RemoveFromCache(item);
                _log.LogInformation("{name} with Id {id} removed from cache", typeof(TEntity).Name, id);
            }
            return result;
        }

        protected virtual string GetKey(object uniqueValue)
        {
            return $"{_cacheKeyPrefix}_{uniqueValue}";
        }

        protected virtual void AddOrUpdateCache(TEntity item)
        {
            _cache.Set(GetKey(item.Id), item, _cacheOptions);
        }

        protected virtual void RemoveFromCache(TEntity item)
        {
            _cache.Remove(GetKey(item.Id));
        }
    }
}
