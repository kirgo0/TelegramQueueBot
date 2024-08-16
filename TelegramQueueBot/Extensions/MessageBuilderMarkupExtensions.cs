using QueueCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramQueueBot.Common;
using TelegramQueueBot.Helpers;

namespace TelegramQueueBot.Extensions
{
    public static class MessageBuilderMarkupExtensions
    {
        public static MessageBuilder AddDefaultQueueMarkup(this MessageBuilder builder, List<string> userNamesQueue)
        {
            if (userNamesQueue is not null && !userNamesQueue.Any())
                throw new ArgumentNullException(nameof(userNamesQueue));

            for (int i = 0; i < userNamesQueue.Count; i++)
            {
                var userName = userNamesQueue[i];
                if(string.IsNullOrWhiteSpace(userName))
                {
                    builder.AddButtonNextRow(
                        $"{i}. __________________",
                        callbackData: $"{Actions.Enqueue}{i}"
                        );
                } else
                {
                    builder.AddButtonNextRow(
                        $"{i}. {userName}",
                        callbackData: $"{Actions.Dequeue}{i}"
                        );
                }
            }
            return builder;
        }

    }
}
