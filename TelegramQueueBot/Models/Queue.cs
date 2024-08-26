using TelegramQueueBot.Services;

namespace TelegramQueueBot.Models
{
    public class Queue : Entity
    {
        public int Size { get; set; }
        public long ChatId { get; set; }

        public string? Name = null;
        public List<long> List { get; set; } = new List<long>();

        public long this[int i]
        {
            get
            {
                if (i > Size || i < 0)
                {
                    throw new IndexOutOfRangeException();
                }
                return List.ElementAtOrDefault(i);
            }
            set
            {
                if (i > Size || i < 0)
                {
                    throw new IndexOutOfRangeException();
                }
                List[i] = value;
            }
        }

        public int IndexOf(long userId)
        {
            return List.IndexOf(userId);
        }

        public bool IndexOf(long userId, out int index)
        {
            index = List.IndexOf(userId);
            return index != -1;
        }

        public bool IsEmpty => List.Count(e => e != QueueService.EmptyQueueMember) == 0;

        public int Count => List.Count(e => e != QueueService.EmptyQueueMember);
        public Queue()
        {

        }

        public Queue(int size)
        {
            if (size < 0) { throw new ArgumentOutOfRangeException("Size can't be less than zero"); }
            Size = size;
            List = new List<long>(new long[size]);
        }

        public Queue(long chatId, int size)
        {
            if (size < 0) { throw new ArgumentOutOfRangeException("Size can't be less than zero"); }
            ChatId = chatId;
            Size = size;
            List = new List<long>(new long[size]);
        }

        public Queue(Queue copyFrom)
        {
            Id = copyFrom.Id;
            Size = copyFrom.Size;
            foreach (var item in copyFrom.List)
            {
                List.Add(item);
            }
        }
    }
}
