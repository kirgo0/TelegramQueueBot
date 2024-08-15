using TelegramQueueBot.Models;

namespace TelegramQueueBot.Data.Abstraction
{
    public interface IRepository<T> where T : Entity, new()
    {
        Task<T> CreateAsync(T item);
        Task<T> GetAsync(string id);
        Task<bool> UpdateAsync(T item);
        Task<bool> DeleteAsync(string id);
    }
}
