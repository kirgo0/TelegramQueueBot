using TelegramQueueBot.Common;

namespace TelegramQueueBot.Helpers.Attributes
{
    public class HandleActionAttribute : HandlerMetadataAttribute
    {
        public HandleActionAttribute(string value) : base(Metatags.HandleAction, value)
        {
        }
    }
}
