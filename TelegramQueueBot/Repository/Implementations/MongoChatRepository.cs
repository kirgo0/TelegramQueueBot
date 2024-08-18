using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using TelegramQueueBot.Data.Abstraction;
using TelegramQueueBot.Data.Repository;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;

namespace TelegramQueueBot.Repository.Implementations
{
    public class MongoChatRepository : MongoRepository<Chat>, IChatRepository
    {
        public MongoChatRepository(IMongoContext mongoContext, ILogger<MongoChatRepository> logger) : base(mongoContext, logger)
        {
        }

        public async Task<Chat> GetByTelegramIdAsync(long id)
        {
            try
            {
                var item = await _items.FindAsync(Builders<Chat>.Filter.Eq(u => u.TelegramId, id));
                return item.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred when getting an object of type {type}", typeof(Chat).Name);
                return null;
            }
        }
    }
}
