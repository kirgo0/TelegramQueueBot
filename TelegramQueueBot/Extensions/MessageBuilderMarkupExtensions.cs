using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramQueueBot.Helpers;

namespace TelegramQueueBot.Extensions
{
    public static class MessageBuilderMarkupExtensions
    {
        public static MessageBuilder AddDefaultQueueMarkup(this MessageBuilder builder, List<string> userNamesQueue)
        {
            if(userNamesQueue is not null && !userNamesQueue.Any()) 
                throw new ArgumentNullException(nameof(userNamesQueue));
            return builder;
        }

    }
}
