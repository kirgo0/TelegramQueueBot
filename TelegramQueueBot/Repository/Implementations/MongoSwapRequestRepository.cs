using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using TelegramQueueBot.Data.Abstraction;
using TelegramQueueBot.Data.Repository;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;

namespace TelegramQueueBot.Repository.Implementations
{

    public class MongoSwapRequestRepository : MongoRepository<SwapRequest>, ISwapRequestRepository
    {
        public MongoSwapRequestRepository(IMongoContext mongoContext, ILogger<MongoSwapRequestRepository> logger) : base(mongoContext, logger)
        {
        }

        public async Task<SwapRequest> CreateOrReplaceAsync(SwapRequest request)
        {
            try
            {
                var item = await _items.FindOneAndDeleteAsync(Builders<SwapRequest>.Filter.And(
                    Builders<SwapRequest>.Filter.Eq(x => x.QueueId, request.QueueId),
                    Builders<SwapRequest>.Filter.Or(
                        Builders<SwapRequest>.Filter.Eq(x => x.FirstTelegramId, request.FirstTelegramId),
                        Builders<SwapRequest>.Filter.Eq(x => x.FirstTelegramId, request.SecondTelegramId)
                    ),
                    Builders<SwapRequest>.Filter.Or(
                        Builders<SwapRequest>.Filter.Eq(x => x.SecondTelegramId, request.SecondTelegramId),
                        Builders<SwapRequest>.Filter.Eq(x => x.SecondTelegramId, request.FirstTelegramId)
                    )
                ));
                return await CreateAsync(request);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred when getting an object of type {type}", typeof(Chat).Name);
                return null;
            }
        }
    }
}
