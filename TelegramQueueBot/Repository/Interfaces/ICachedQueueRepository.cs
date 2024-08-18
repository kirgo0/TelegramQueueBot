using TelegramQueueBot.Helpers;

namespace TelegramQueueBot.Repository.Interfaces
{
    public interface ICachedQueueRepository : IQueueRepository
    {
        event EventHandler<QueueUpdatedEventArgs> QueueUpdateEvent;
        IQueueRepository InnerRepository { get; }
    }
}
