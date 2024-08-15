﻿using TelegramQueueBot.Data.Abstraction;
using TelegramQueueBot.Models;

namespace TelegramQueueBot.Repository.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByTelegramIdAsync(long id);
    }
}
