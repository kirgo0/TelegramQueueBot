using Data.Repository;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;

namespace TelegramQueueBot.Repository.Implementations.Cached
{
    public class CachedMongoUserRepository : CachedMongoRepository<MongoUserRepository, User>, IUserRepository
    {
        public CachedMongoUserRepository(MongoUserRepository innerRepository, ILogger<CachedMongoUserRepository> log, IMemoryCache cache, MemoryCacheEntryOptions cacheOptions) : base(innerRepository, log, cache, cacheOptions)
        {
        }

        public async Task<User> GetByTelegramIdAsync(long id)
        {
            if (id == 0)
            {
                _log.LogDebug("Invalid Telegram ID provided, returning null.");
                return null;
            }

            // Try to retrieve the user from the cache
            if (_cache.TryGetValue(GetKey(id), out User? cachedUser))
            {
                _log.LogDebug("User with Telegram ID {id} retrieved from cache.", id);
                return cachedUser;
            }

            // User not found in cache, retrieve from the database
            var user = await _innerRepository.GetByTelegramIdAsync(id);

            if (user != null)
            {
                AddOrUpdateCache(user);
                _log.LogDebug("User with Telegram ID {id} added to cache.", id);
            }

            return user;
        }
        public async Task<List<User>> GetByTelegramIdsAsync(List<long> telegramIds)
        {
            var resultUsers = new List<User>();

            if (!telegramIds.Any())
            {
                _log.LogDebug("No valid Telegram IDs provided, returning an empty list");
                return resultUsers;
            }

            // List to keep track of IDs that are not found in the cache
            var missingIds = new List<long>();

            // Attempt to retrieve users from the cache
            foreach (var id in telegramIds)
            {
                if (!_cache.TryGetValue(GetKey(id), out User cachedUser))
                {
                    missingIds.Add(id);
                }
            }


            // If there are any missing IDs, fetch those users from the database
            missingIds.RemoveAll(item => item == 0);
            if (missingIds.Any())
            {
                var dbUsers = await _innerRepository.GetByTelegramIdsAsync(missingIds);
                if (dbUsers.Count != missingIds.Count)
                {
                    _log.LogError("Not all users were retrieved from the database, probably an out-of-date queue");
                }
                foreach (var user in dbUsers)
                {
                    AddOrUpdateCache(user);
                    _log.LogDebug("User with TelegramId {id} added to cache", user.TelegramId);
                }
            }

            // Combine cached users and users fetched from the database, maintaining the original order
            foreach (var id in telegramIds)
            {
                _cache.TryGetValue(GetKey(id), out User user);
                resultUsers.Add(user);
            }

            return resultUsers;
        }

        protected override void AddOrUpdateCache(User item)
        {
            _cache.Set(GetKey(item.Id), item, _cacheOptions);
            _cache.Set(GetKey(item.TelegramId), item, _cacheOptions);
        }

        protected override void RemoveFromCache(User item)
        {
            _cache.Remove(GetKey(item.Id));
            _cache.Remove(GetKey(item.TelegramId));
        }
    }
}
