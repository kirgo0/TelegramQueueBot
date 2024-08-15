using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using TelegramQueueBot.Data.Abstraction;
using TelegramQueueBot.Models;

namespace TelegramQueueBot.Data.Repository
{
    public class MongoRepository<T> : IRepository<T> where T : Entity, new()
    {
        private readonly IMongoContext _mongoContext;
        protected readonly IMongoCollection<T> _items;
        protected readonly ILogger _log;
        public MongoRepository(IMongoContext mongoContext, ILogger logger)
        {
            _mongoContext = mongoContext;
            _items = _mongoContext.GetCollection<T>(typeof(T).Name);
            _log = logger;
        }

        public async Task<T> CreateAsync(T item)
        {
            try
            {
                await _items.InsertOneAsync(item);
                if (item is not null) _log.LogDebug("Successfuly inserted new {type}", typeof(T).Name);
                return item;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred when creating an object of type {type}", typeof(T).Name);
                return null;
            }
        }

        public async Task<T> GetAsync(string id)
        {
            try
            {
                var item = await _items.FindAsync(Builders<T>.Filter.Eq("_id", id));
                return item.Single();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred when getting an object of type {type}", typeof(T).Name);
                return null;
            }
        }

        public async Task<bool> UpdateAsync(T item)
        {
            try
            {
                var operationResult = await _items.ReplaceOneAsync(Builders<T>.Filter.Eq("_id", item.Id), item);
                var result = operationResult.IsAcknowledged && operationResult.ModifiedCount > 0;
                if (result) _log.LogDebug("Successfuly update {type} with id {id}", typeof(T).Name, item.Id);
                return result;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred when updating an object of type {type}", typeof(T).Name);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var operationResult = await _items.DeleteOneAsync(Builders<T>.Filter.Eq("_id", id));
                var result = operationResult.IsAcknowledged && operationResult.DeletedCount > 0;
                if (result) _log.LogDebug("Successufy deleted {type} with id {id}", typeof(T).Name, id);
                return result;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred when deleting an object of type {type}", typeof(T).Name);
                return false;
            }
        }

    }
}
