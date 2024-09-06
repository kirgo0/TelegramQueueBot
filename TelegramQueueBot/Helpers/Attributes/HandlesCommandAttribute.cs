using TelegramQueueBot.Common;

namespace TelegramQueueBot.Helpers.Attributes
{
    public class HandlesCommandAttribute : HandlerMetadataAttribute
    {
        public HandlesCommandAttribute(string value) : base(Metatags.HandleCommand, value)
        {
        }
    }
}
