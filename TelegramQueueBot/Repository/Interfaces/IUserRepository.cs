using TelegramQueueBot.Data.Abstraction;
using TelegramQueueBot.Models;

namespace TelegramQueueBot.Repository.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByTelegramIdAsync(long id);
        Task<List<User>> GetByTelegramIdsAsync(List<long> telegramIds);
        Task<List<User>> GetUsersWithAllowedNotificationsAsync(string chatId);
    }
}
