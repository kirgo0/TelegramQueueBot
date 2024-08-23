using TelegramQueueBot.Common;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Models;

namespace TelegramQueueBot.Extensions
{
    public static class MessageBuilderMarkupExtensions
    {

        public static MessageBuilder AddDefaultQueueMarkup(this MessageBuilder builder, List<User> usersQueue)
        {
            if (usersQueue is null || !usersQueue.Any())
                throw new ArgumentNullException(nameof(usersQueue));

            for (int i = 0; i < usersQueue.Count; i++)
            {
                var user = usersQueue[i];
                if (user is null)
                {
                    builder.AddButtonNextRow(
                        $"{i}. __________________",
                        callbackData: $"{Actions.Enqueue}{i}"
                        );
                }
                else
                {
                    builder.AddButtonNextRow(
                        $"{i}. {user.UserName}",
                        callbackData: $"{Actions.Dequeue}{user.TelegramId}"
                        );
                }
            }
            return builder;
        }

        public static MessageBuilder AddSavedQueueMarkup(this MessageBuilder builder, List<User> usersQueue)
        {
            if (usersQueue is null || !usersQueue.Any())
                throw new ArgumentNullException(nameof(usersQueue));

            for (int i = 0; i < usersQueue.Count; i++)
            {
                var user = usersQueue[i];
                if (user is null)
                {
                    builder.AddButtonNextRow(
                        $"{i}. __________________",
                        callbackData: $"_"
                        );
                }
                else
                {
                    builder.AddButtonNextRow(
                        $"{i}. {user.UserName}",
                        callbackData: $"_"
                        );
                }
            }
            return builder;
        }

    }
}
