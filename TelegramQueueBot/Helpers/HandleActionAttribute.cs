using TelegramQueueBot.Common;

namespace TelegramQueueBot.Helpers
{
    public class HandleActionAttribute : HandlerMetadataAttribute
    {
        public HandleActionAttribute(string value) : base(Metatags.HandleAction, value)
        {
        }
    }
}
