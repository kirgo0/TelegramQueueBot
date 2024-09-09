using TelegramQueueBot.Common;

namespace TelegramQueueBot.Helpers.Attributes
{
    public class HandleCommandAttribute : HandlerMetadataAttribute
    {
        public HandleCommandAttribute(string value) : base(Metatags.HandleCommand, value)
        {
        }
    }
}
