using TelegramQueueBot.Models.Enums;

namespace TelegramQueueBot.Models
{
    public class Chat : Entity
    {
        public long TelegramId { get; set; }
        public int LastMessageId { get; set; }
        public List<string> QueueList { get; set; } = new List<string>();
        public string CurrentQueueId { get; set; } = string.Empty;
        public int DefaultQueueSize { get; set; } = 10;
        public ChatMode Mode { get; set; } = ChatMode.Open;
        public ViewType View { get; set; } = ViewType.Column;

        public Chat(long telegramId)
        {
            TelegramId = telegramId;
        }

        public Chat() { }

    }
}
