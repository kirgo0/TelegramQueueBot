using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramQueueBot.Models;

namespace TelegramQueueBot.Helpers
{
    public class QueueUpdatedEventArgs : EventArgs
    {
        public QueueUpdatedEventArgs(Queue queue)
        {
            Queue = queue;
        }
        public Queue Queue { get; }
    }
}
