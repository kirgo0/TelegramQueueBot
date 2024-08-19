using TelegramQueueBot.Helpers;
using TelegramQueueBot.Models;

namespace TelegramQueueBot.Repository.Interfaces
{
    public interface ICachedQueueRepository : IQueueRepository
    {
        event EventHandler<QueueUpdatedEventArgs> QueueUpdateEvent;
        IQueueRepository InnerRepository { get; }
        Task<bool> UpdateAsync(Queue item, bool doRender);
    }
}
