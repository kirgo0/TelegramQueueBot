using Data.Repository;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;

namespace TelegramQueueBot.Repository.Implementations
{
    public class CachedMongoQueueRepository : CachedMongoRepository<MongoQueueRepository, Queue>, ICachedQueueRepository
    {
        public IQueueRepository InnerRepository => _innerRepository;
        public event EventHandler<QueueUpdatedEventArgs> QueueUpdateEvent;

        public CachedMongoQueueRepository(MongoQueueRepository innerRepository, ILogger<CachedMongoQueueRepository> log, IMemoryCache cache) : base(innerRepository, log, cache)
        {
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
                    _cache.Set(queue.Id, queue);
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
            _cache.Set(item.Id, item);
            return Task.FromResult(true);
        }

        public async Task<bool> UpdateAsync(Queue item, bool doRender)
        {
            if (doRender)
            {
                OnQueueUpdatedEvent(new QueueUpdatedEventArgs(item));
                _log.LogDebug("An {event} has been triggered in the queue with identifier {id}", item.Id, nameof(QueueUpdateEvent));
                _cache.Set(item.Id, item);
                return true;
            } else
            {
                var result = await InnerRepository.UpdateAsync(item);
                if (result)
                {
                    _cache.Set(item.Id, item);
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
