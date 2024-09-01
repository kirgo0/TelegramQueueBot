namespace TelegramQueueBot.Models
{
    public class ChatJob : Entity
    {
        public long ChatId { get; set; }
        public string? QueueId { get; set; }
        public string JobId { get; set; } = string.Empty;
        public string JobName { get; set; } = string.Empty;
        public string CronExpression { get; set; } = string.Empty;
        public int Interval { get; set; } = 1;
        public DateTime LastRunTime { get; set; }
        public DateTime NextRunTime { get; set; }
    }
}
