using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramQueueBot.DataAccess.Abstraction;
using TelegramQueueBot.Models;

namespace TelegramQueueBot.DataAccess.Repository
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
                _log.LogError(ex, "An error occurred when getting an object of type {type}", typeof(User).Name);
                return null;
            }
        }
    }
}
