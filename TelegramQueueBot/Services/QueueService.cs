using System.Collections.Concurrent;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;

namespace TelegramQueueBot.Services
{

    public class QueueService
    {
        public static int EmptyQueueMember { get; } = 0;

        private readonly ICachedQueueRepository _queueRepository;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphoreDictionary = new ConcurrentDictionary<string, SemaphoreSlim>();

        public QueueService(ICachedQueueRepository queueRepository)
        {
            _queueRepository = queueRepository;
        }

        protected SemaphoreSlim GetSemaphore(string queueId)
        {
            return _semaphoreDictionary.GetOrAdd(queueId, _ => new SemaphoreSlim(1, 1));
        }

        protected void RemoveSemaphore(string queueId)
        {
            _semaphoreDictionary.Remove(queueId, out _);
        }

        public async Task<Queue> GetQueueSnapshotAsync(string queueId)
        {
            Queue result = null;

            await AccessQueueAsync(queueId, (queue) =>
            {
                result = new Queue(queue);
                return false;
            });

            return result;
        }

        public async Task<bool> DoThreadSafeWorkOnQueueAsync(string queueId, Action<Queue> workToDo)
        {
            var semaphore = GetSemaphore(queueId);
            await semaphore.WaitAsync();
            try
            {
                var queue = await _queueRepository.GetAsync(queueId);
                // checking if queue wasn't found
                if (queue is null) return false;
                workToDo.Invoke(queue);
                return true;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task<bool> DoThreadSafeWorkOnQueueAsync(string queueToLockId, Action workToDo)
        {
            var semaphore = GetSemaphore(queueToLockId);
            await semaphore.WaitAsync();
            try
            {
                workToDo.Invoke();
                return true;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task<Queue> CreateQueueAsync(long chatId)
        {
            var queue = await _queueRepository.CreateAsync(chatId);
            return queue;
        }
        public async Task<Queue> CreateQueueAsync(long chatId, int size)
        {
            var queue = await _queueRepository.CreateAsync(chatId, size);
            return queue;
        }

        public async Task<bool> DeleteQueueAsync(string queueId)
        {
            var semaphore = GetSemaphore(queueId);
            await semaphore.WaitAsync();
            try
            {
                var result = await _queueRepository.DeleteAsync(queueId);

                if (result)
                {
                    RemoveSemaphore(queueId);
                }
                return result;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task<bool> EnqueueAsync(string queueId, int position, long userId, bool doRender = true)
        {
            if (userId < 0 || position < 0)
                throw new ArgumentOutOfRangeException("Argument values cannot be less than zero");

            return await AccessQueueAsync(queueId, (queue) =>
            {
                // checking for bad requests
                if (position > queue.Size) return false;

                // checking whether the user already takes this place in the queue
                if (queue[position] == userId) return false;

                // checking whether the user is already in the queue then dequeue him
                if (queue.IndexOf(userId, out int userCurrentPos))
                {
                    queue[userCurrentPos] = EmptyQueueMember;
                }
                // checking whether the position is empty and putting the user in the queue
                if (queue[position] == EmptyQueueMember)
                {
                    queue[position] = userId;
                    return true;
                }
                else
                {
                    return false;
                }
            }, doRender);

        }

        public async Task<bool> DequeueAsync(string queueId, long userId, bool doRender = true)
        {
            if (userId < 0)
                throw new ArgumentOutOfRangeException(nameof(userId), "The id values cannot be less than zero");

            return await AccessQueueAsync(queueId, (queue) =>
            {
                // checking if user is in queue
                if (!queue.IndexOf(userId, out int userPos)) return false;

                queue[userPos] = EmptyQueueMember;
                return true;
            }, doRender);

        }

        public async Task<bool> DequeueFirstAsync(string queueId, bool doRender = true)
        {

            return await AccessQueueAsync(queueId, (queue) =>
            {
                try
                {
                    var firstUserId = queue.List.First(u => u != EmptyQueueMember);
                    queue.List.Remove(firstUserId);
                    queue.List.Add(EmptyQueueMember);
                    return true;
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            },doRender);
        }

        public async Task<bool> MoveUserUpAsync(string queueId, long userId, bool doRender = true)
        {
            if (userId < 0)
                throw new ArgumentOutOfRangeException(nameof(userId), "The id value cannot be less than zero");

            return await AccessQueueAsync(queueId, (queue) =>
            {
                var userPos = queue.IndexOf(userId);

                if (userPos <= 0) return false;

                var temp = queue[userPos - 1];
                queue[userPos - 1] = userId;
                queue[userPos] = temp;

                return true;
            }, doRender);
        }

        public async Task<bool> SwapUsersAsync(string queueId, long firstUserId, long secondUserId, bool doRender = true)
        {
            if (firstUserId < 0 || secondUserId < 0)
                throw new ArgumentOutOfRangeException("The id value cannot be less than zero");

            if (firstUserId == secondUserId)
                return false;

            return await AccessQueueAsync(queueId, (queue) =>
            {
                int firstIndex = queue.IndexOf(firstUserId);
                int secondIndex = queue.IndexOf(secondUserId);

                if (firstIndex < 0 || secondIndex < 0)
                    return false;

                var temp = queue[firstIndex];
                queue[firstIndex] = queue[secondIndex];
                queue[secondIndex] = temp;
                return true;
            }, doRender);

        }

        public async Task<bool> SetQueueSizeAsync(string queueId, int size, bool doRender = true)
        {
            if (size <= 1)
                throw new ArgumentOutOfRangeException("The queue size must be greater than 0");

            return await AccessQueueAsync(queueId, (queue) =>
            {
                if (queue.Size == size) return false;

                if (queue.Size > size)
                {
                    var queueCopy = new Queue(queue);
                    queueCopy.List.Reverse();
                    for (int i = 0; i < queueCopy.Size - size; i++)
                    {
                        if (queueCopy.List.Remove(0)) continue;
                        else break;
                    }
                    queueCopy.List.Reverse();
                    if (queueCopy.List.Count > size)
                    {
                        return false;
                    }
                    else
                    {
                        queue.List = queueCopy.List;
                    }
                }
                else
                {
                    queue.List.AddRange(new long[size - queue.Size]);
                }

                queue.Size = size;
                return true;
            }, doRender);

        }

        public async Task<bool> RemoveBlankSpacesAsync(string queueId, bool doRender = true)
        {

            return await AccessQueueAsync(queueId, (queue) =>
            {
                var nonEmptyCount = queue.List.Count(i => i != EmptyQueueMember);

                var removedCount = 0;
                for (int i = 0; i < queue.List.Count - removedCount && nonEmptyCount > 0; i++)
                {
                    var item = queue.List[i];
                    if (item == EmptyQueueMember)
                    {
                        queue.List.RemoveAt(i);
                        queue.List.Add(EmptyQueueMember);
                        removedCount++;
                        i--;
                    }
                    else
                    {
                        nonEmptyCount--;
                    }
                }
                if (removedCount > 0) return true;
                return false;
            }, doRender);

        }

        protected async Task<bool> AccessQueueAsync(string queueId, Func<Queue, bool> queueWork, bool doRender = true)
        {
            var semaphore = GetSemaphore(queueId);
            await semaphore.WaitAsync();
            try
            {
                var queue = await _queueRepository.GetAsync(queueId);
                // checking if queue wasn't found
                if (queue is null) return false;

                // the invoked method should return true if any changes have occurred in the queue object
                var result = queueWork.Invoke(queue);
                // saving changes if returned true
                if (result)
                {
                    await _queueRepository.UpdateAsync(queue, doRender);
                }
                return result;
            }
            finally
            {
                semaphore.Release();
            }
        }


    }
}
