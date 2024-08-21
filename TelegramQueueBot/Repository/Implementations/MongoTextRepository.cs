using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using TelegramQueueBot.Data.Abstraction;
using TelegramQueueBot.Data.Repository;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;

namespace TelegramQueueBot.Repository.Implementations
{
    public class MongoTextRepository : MongoRepository<Text>, ITextRepository
    {
        public MongoTextRepository(IMongoContext mongoContext, ILogger<MongoTextRepository> logger) : base(mongoContext, logger)
        {
        }

        public async Task<Text> GetByKeyAsync(string key)
        {
            try
            {
                var item = await _items.FindAsync(Builders<Text>.Filter.Eq(e => e.Key, key));
                return item.Single();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred when getting an object of type {type}", typeof(Text).Name);
                return null;
            }
        }
    }

}
