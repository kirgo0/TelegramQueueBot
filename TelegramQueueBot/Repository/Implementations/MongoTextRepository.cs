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
        public static Text NotFoundText = new Text() { Value = "Text is not found" };
        public MongoTextRepository(IMongoContext mongoContext, ILogger<MongoTextRepository> logger) : base(mongoContext, logger)
        {
        }

        public async Task<Text> GetByKeyAsync(string key)
        {
            try
            {
                var item = await _items.FindAsync(Builders<Text>.Filter.Eq(e => e.Key, key));
                var result = item.FirstOrDefault();
                if (result is null)
                {
                    _log.LogWarning("Text not found for the specified key value {key}", key);
                    return NotFoundText;
                }
                else
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred when getting an object of type {type}", typeof(Text).Name);
                return null;
            }
        }

        public virtual async Task<string> GetValueAsync(string key)
        {
            var text = await GetByKeyAsync(key);
            return text is not null ? text.Value : string.Empty;
        }
    }

}
