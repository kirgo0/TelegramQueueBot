using Data.Repository;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Implementations;
using TelegramQueueBot.Repository.Interfaces;

namespace TelegramQueueBot.Repository.Implementations.Cached
{
    public class CachedMongoSwapRequestRepository : CachedMongoRepository<MongoSwapRequestRepository, SwapRequest>, ISwapRequestRepository
    {
        public CachedMongoSwapRequestRepository(MongoSwapRequestRepository innerRepository, ILogger<CachedMongoSwapRequestRepository> log, IMemoryCache cache, MemoryCacheEntryOptions cacheOptions) : base(innerRepository, log, cache, cacheOptions)
        {
        }

        public async Task<SwapRequest> CreateOrReplaceAsync(SwapRequest request)
        {
            var result = await _innerRepository.CreateOrReplaceAsync(request);
            if(result is not null)
            {
                AddOrUpdateCache(result);
                _log.LogDebug("Swap request with id {id} added to cache", result.Id);
            }
            return result;
        }
    }
}