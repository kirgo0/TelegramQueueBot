using TelegramQueueBot.Data.Abstraction;
using TelegramQueueBot.Models;

namespace TelegramQueueBot.Repository.Interfaces
{
    public interface IQueueRepository : IRepository<Queue>
    {
        Task<Queue> CreateAsync(long chatId);
        Task<Queue> CreateAsync(long chatId, int size);
    }
}
