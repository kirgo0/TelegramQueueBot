using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using TelegramQueueBot.Data.Abstraction;
using TelegramQueueBot.Models;

namespace TelegramQueueBot.Data.Repository
{
    public class MongoRepository<TEntity> : IRepository<TEntity> where TEntity : Entity, new()
    {
        private readonly IMongoContext _mongoContext;
        protected readonly IMongoCollection<TEntity> _items;
        protected readonly ILogger _log;
        public MongoRepository(IMongoContext mongoContext, ILogger logger)
        {
            _mongoContext = mongoContext;
            _items = _mongoContext.GetCollection<TEntity>(typeof(TEntity).Name);
            _log = logger;
        }

        public async Task<TEntity> CreateAsync(TEntity item)
        {
            try
            {
                await _items.InsertOneAsync(item);
                if (item is not null)
                {
                    _log.LogInformation("Successfuly inserted new {type}", typeof(TEntity).Name);
                }
                return item;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred when creating an object of type {type}", typeof(TEntity).Name);
                return null;
            }
        }

        public async Task<TEntity> GetAsync(string id)
        {
            try
            {
                var item = await _items.FindAsync(Builders<TEntity>.Filter.Eq(e => e.Id, id));
                return item.SingleOrDefault();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred when getting an object of type {type}", typeof(TEntity).Name);
                return null;
            }
        }

        public async Task<bool> UpdateAsync(TEntity item)
        {
            try
            {
                var operationResult = await _items.ReplaceOneAsync(Builders<TEntity>.Filter.Eq(e => e.Id, item.Id), item);
                var result = operationResult.IsAcknowledged && operationResult.ModifiedCount > 0;
                if (result) _log.LogInformation("Successfuly updated {type} with id {id}", typeof(TEntity).Name, item.Id);
                return result;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred when updating an object of type {type}", typeof(TEntity).Name);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var operationResult = await _items.DeleteOneAsync(Builders<TEntity>.Filter.Eq(e => e.Id, id));
                var result = operationResult.IsAcknowledged && operationResult.DeletedCount > 0;
                if (result) _log.LogInformation("Successfuly deleted {type} with id {id}", typeof(TEntity).Name, id);
                return result;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred when deleting an object of type {type}", typeof(TEntity).Name);
                return false;
            }
        }

    }
}
