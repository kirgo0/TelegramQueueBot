using Microsoft.Extensions.Logging;
using TelegramQueueBot.Data.Abstraction;
using TelegramQueueBot.Data.Repository;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;

namespace TelegramQueueBot.Repository.Implementations
{
    public class ChatJobMongoRepository : MongoRepository<ChatJob>, IChatJobRepository
    {
        public ChatJobMongoRepository(IMongoContext mongoContext, ILogger<ChatJobMongoRepository> logger)
            : base(mongoContext, logger)
        {
        }

        public Task<List<ChatJob>> GetAllByChatIdAsync(long chatId)
        {

        }
    }

}
