namespace TelegramQueueBot.Helpers
{
    public class HandlerMetadataAttribute : Attribute
    {
        public string Key { get; }
        public object Value { get; }

        public HandlerMetadataAttribute(string key, object value)
        {
            Key = key;
            Value = value;
        }
    }
}
