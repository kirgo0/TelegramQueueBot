using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramQueueBot.Models.Enums;

namespace TelegramQueueBot.Models
{
    public class Chat
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        public List<string> QueueList { get; set; } = new List<string>();
        public int DefaultQueueSize { get; set; } = 10;
        public ChatState ChatIs { get; set; } = ChatState.Open;
        public ViewType View { get; set; } = ViewType.Column;

    }
}
