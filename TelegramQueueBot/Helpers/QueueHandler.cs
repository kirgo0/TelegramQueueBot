using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramQueueBot.Helpers
{
    public class QueueHandler
    {
        public static List<string> GetQueueNames(List<long> queue, Dictionary<long, string> names)
        {
            var result = new List<string>();
            foreach (var id in queue)
            {
                var item = string.Empty;
                if (names.TryGetValue(id, out string name)) item = name;
                result.Add(item);
            }
            return result;
        }
    }
}
