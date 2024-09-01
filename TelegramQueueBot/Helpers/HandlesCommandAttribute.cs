using TelegramQueueBot.Common;

namespace TelegramQueueBot.Helpers
{
    public class HandlesCommandAttribute : HandlerMetadataAttribute
    {
        public HandlesCommandAttribute(string value) : base(Metatags.HandleCommand, value)
        {
        }
    }
}
