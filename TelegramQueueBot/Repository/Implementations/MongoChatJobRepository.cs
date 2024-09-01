using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using TelegramQueueBot.Data.Abstraction;
using TelegramQueueBot.Data.Repository;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;

namespace TelegramQueueBot.Repository.Implementations
{
    public class MongoChatJobRepository : MongoRepository<ChatJob>, IChatJobRepository
    {
        public MongoChatJobRepository(IMongoContext mongoContext, ILogger<MongoChatJobRepository> logger)
            : base(mongoContext, logger)
        {
        }

        public async Task<List<ChatJob>> GetAllByChatIdAsync(long chatId)
        {
            try
            {
                var filter = Builders<ChatJob>.Filter.Eq(c => c.ChatId, chatId);
                var jobs = await _items.Find(filter).ToListAsync();
                return jobs;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred when retrieving chat jobs for chat {chatId}", chatId);
                return new List<ChatJob>();
            }
        }
    }

}
