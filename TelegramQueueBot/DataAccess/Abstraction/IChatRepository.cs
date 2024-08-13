using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramQueueBot.Models;

namespace TelegramQueueBot.DataAccess.Abstraction
{
    public interface IChatRepository : IRepository<Chat>
    {
    }
}
