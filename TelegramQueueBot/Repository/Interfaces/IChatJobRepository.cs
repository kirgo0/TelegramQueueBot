using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramQueueBot.Data.Abstraction;
using TelegramQueueBot.Models;

namespace TelegramQueueBot.Repository.Interfaces
{
    public interface IChatJobRepository : IRepository<ChatJob>
    {
        public Task<List<ChatJob>> GetAllByChatIdAsync(long chatId);
    }
}
