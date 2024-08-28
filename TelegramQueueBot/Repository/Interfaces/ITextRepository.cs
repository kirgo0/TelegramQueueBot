using TelegramQueueBot.Data.Abstraction;
using TelegramQueueBot.Models;

namespace TelegramQueueBot.Repository.Interfaces
{
    public interface ITextRepository : IRepository<Text>
    {
        Task<Text> GetByKeyAsync(string key);
        Task<string> GetValueAsync(string key);
    }
}
