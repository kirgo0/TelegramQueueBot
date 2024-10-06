using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramQueueBot.Models
{
    public class SwapRequest : Entity
    {
        public string QueueId { get; set; } = string.Empty;
        public long FirstTelegramId { get; set; }
        public int FirstPosition { get; set; }
        public long SecondTelegramId { get; set; }
        public int SecondPosition { get; set; }

    }
}
