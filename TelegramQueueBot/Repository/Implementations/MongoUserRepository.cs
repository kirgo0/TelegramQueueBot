using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using TelegramQueueBot.Data.Abstraction;
using TelegramQueueBot.Data.Repository;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;

namespace TelegramQueueBot.Repository.Implementations
{
    public class MongoUserRepository : MongoRepository<User>, IUserRepository
    {
        public MongoUserRepository(IMongoContext mongoContext, ILogger<MongoUserRepository> logger) : base(mongoContext, logger)
        {
        }

        public async Task<User> GetByTelegramIdAsync(long id)
        {
            try
            {
                var item = await _items.FindAsync(Builders<User>.Filter.Eq(u => u.TelegramId, id));
                return item.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred when getting an object of type {type}", typeof(User).Name);
                return null;
            }
        }

        public async Task<Dictionary<long, string>> GetUsernamesAsync(List<long> telegramIds)
        {
            try
            {
                telegramIds = telegramIds.Where(id => id != 0).ToList();

                if (!telegramIds.Any())
                {
                    _log.LogDebug("No valid Telegram IDs provided, returning an empty dictionary.");
                    return new Dictionary<long, string>();
                }

                var filter = Builders<User>.Filter.In(u => u.TelegramId, telegramIds);
                var users = await _items.Find(filter).ToListAsync();

                return users.ToDictionary(u => u.TelegramId, u => u.UserName);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred when retrieving usernames for users with specified Telegram IDs.");
                return new Dictionary<long, string>();
            }
        }
    }
}
