using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramQueueBot.DataAccess.Abstraction;
using TelegramQueueBot.Models;

namespace TelegramQueueBot.DataAccess.Repository
{
    public class MongoChatRepository : MongoRepository<Chat>, IChatRepository
    {
        public MongoChatRepository(IMongoContext mongoContext, ILogger logger) : base(mongoContext, logger)
        {
        }
    }
}
