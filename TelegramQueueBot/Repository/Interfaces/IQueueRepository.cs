using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
