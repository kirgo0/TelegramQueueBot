using TelegramQueueBot.Data.Abstraction;
using TelegramQueueBot.Models;

namespace TelegramQueueBot.Repository.Interfaces
{
    public interface IChatRepository : IRepository<Chat>
    {
        Task<Chat> GetByTelegramIdAsync(long id);
    }
}
