namespace TelegramQueueBot.Models
{
    public class User : Entity
    {
        public long TelegramId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public bool IsAuthorized { get; set; } = false;
        public bool SendNotifications { get; set; } = false;

        public User(long telegramId, string userName)
        {
            TelegramId = telegramId;
            UserName = userName;
        }

        public User() { }
    }
}
