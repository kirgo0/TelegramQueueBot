using Microsoft.Extensions.Logging;
using MongoDB.Driver;
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

        public async Task<List<Queue>> GetByIdsAsync(List<string> queueIds)
        {
            try
            {
                var result = await _items.Find(Builders<Queue>.Filter.In(q => q.Id, queueIds)).ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred when getting objects if type {type} by Ids", typeof(Queue).Name);
                return new List<Queue>();
            }
        }
    }
}
