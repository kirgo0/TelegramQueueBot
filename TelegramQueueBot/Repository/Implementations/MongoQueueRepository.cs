using Microsoft.Extensions.Logging;
using TelegramQueueBot.Data.Abstraction;
using TelegramQueueBot.Data.Repository;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;

namespace TelegramQueueBot.Repository.Implementations
{
    public class MongoQueueRepository : MongoRepository<Queue>, IQueueRepository
    {
        public MongoQueueRepository(IMongoContext mongoContext, ILogger<MongoQueueRepository> logger) : base(mongoContext, logger)
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
                var queue = new Queue(chatId, size);
                await _items.InsertOneAsync(queue);
                return queue;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred when creating an object of type {type}", GetType().Name);
                return null;
            }
        }


    }
}
