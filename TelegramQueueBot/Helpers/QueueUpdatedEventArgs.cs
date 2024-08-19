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

        public bool AsNewMessage { get; set; } = false;
    }
}
