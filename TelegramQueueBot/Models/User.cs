using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramQueueBot.Models
{
    public class User : Entity
    {
        public long TelegramId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public bool IsAuthorized { get; set; } = false;

        public User(long telegramId, string userName)
        {
            TelegramId = telegramId;
            UserName = userName;
        }

        public User() { }
    }
}
