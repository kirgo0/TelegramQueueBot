using Data.Repository;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Telegram.Bot.Types;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;

namespace TelegramQueueBot.Repository.Implementations
{
    public class CachedMongoQueueRepository : CachedMongoRepository<MongoQueueRepository, Queue>, ICachedQueueRepository
    {
        public IQueueRepository InnerRepository => _innerRepository;
        public event EventHandler<QueueUpdatedEventArgs> QueueUpdateEvent;

        public CachedMongoQueueRepository(MongoQueueRepository innerRepository, ILogger<CachedMongoQueueRepository> log, IMemoryCache cache, TimeSpan cacheDuration) : base(innerRepository, log, cache, cacheDuration)
        {
        }

        public async Task<List<Queue>> GetByIdsAsync(List<string> queueIds)
        {
            var resultQueues = new List<Queue>();

            if (queueIds is null || !queueIds.Any())
            {
                _log.LogDebug("No valid queue Ids provided, returning an empty list");
                return resultQueues;
            }

            var missingIds = new List<string>();
            foreach (var id in queueIds)
            {
                if (!_cache.TryGetValue(GetKey(id), out var queue))
                {
                    missingIds.Add(id);
                }
            }

            try
            {
                if (missingIds.Any())
                {
                    var dbQueues = await InnerRepository.GetByIdsAsync(missingIds);
                    if (dbQueues.Count != missingIds.Count)
                    {
                        _log.LogError("Not all queues were retrieved from the database, probably an out-of-date chat");
                    }
                    foreach (var queue in dbQueues)
                    {
                        _cache.Set(GetKey(queue.Id), queue, _cacheDuration);
                        _log.LogDebug("Queue with Id {id} added to cache", queue.Id);
                    }
                }
                foreach(var id in queueIds)
                {
                    if(_cache.TryGetValue(GetKey(id), out Queue queue))
                    {
                        resultQueues.Add(queue);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred when retrieving queues with specified Ids");
            }
            return resultQueues;

        }

        public async Task<Queue> CreateAsync(long chatId)
        {
            return await CreateAsync(chatId, 10);
        }

        public async Task<Queue> CreateAsync(long chatId, int size)
        {
            try
            {
                var queue = await InnerRepository.CreateAsync(chatId, size);
                if (queue is not null)
                {
                    _cache.Set(GetKey(queue.Id), queue, _cacheDuration);
                    _log.LogDebug("{name} with Id {id} added to cache", nameof(Queue), queue.Id);
                }
                return queue;
            }
            catch (Exception ex)
            {
                _log.LogError("An error occured while creating an object of type {type}", GetType().Name);
                return null;
            }
        }

        public override Task<bool> UpdateAsync(Queue item)
        {
            OnQueueUpdatedEvent(new QueueUpdatedEventArgs(item));
            _log.LogDebug("An {event} has been triggered in the queue with identifier {id}", item.Id, nameof(QueueUpdateEvent));
            _cache.Set(GetKey(item.Id), item, _cacheDuration);
            return Task.FromResult(true);
        }

        public async Task<bool> UpdateAsync(Queue item, bool doRender)
        {
            if (doRender)
            {
                OnQueueUpdatedEvent(new QueueUpdatedEventArgs(item));
                _log.LogDebug("An {event} has been triggered in the queue with identifier {id}", item.Id, nameof(QueueUpdateEvent));
                _cache.Set(GetKey(item.Id), item, _cacheDuration);
                return true;
            }
            else
            {
                var result = await InnerRepository.UpdateAsync(item);
                if (result)
                {
                    _cache.Set(GetKey(item.Id), item, _cacheDuration);
                    _log.LogDebug("{name} with Id {id} updated in cache", typeof(Queue).Name, item.Id);
                }
                return result;
            }
        }

        protected virtual void OnQueueUpdatedEvent(QueueUpdatedEventArgs e)
        {
            QueueUpdateEvent?.Invoke(this, e);
        }
    }
}
