using MongoDB.Driver;

namespace TelegramQueueBot.Data.Abstraction
{
    public interface IMongoContext
    {
        public IMongoCollection<T> GetCollection<T>(string name);
    }
}
