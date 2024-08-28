using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramQueueBot.Models
{
    public class ChatJob : Entity
    {
        public long ChatId { get; set; }
        public string JobId { get; set; } = string.Empty;
        public string JobName { get; set; } = string.Empty;
        public string CronExpression { get; set; } = string.Empty;
        public DateTime LastRunTime { get; set; }
        public DateTime NextRunTime { get; set; }
    }
}
