using TelegramQueueBot.Common;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Models;
using TelegramQueueBot.Models.Enums;

namespace TelegramQueueBot.Extensions
{
    public static class MessageBuilderMarkupExtensions
    {
        //public static readonly string EmptyQueueValue = Enumerable.Repeat("_", 10).Aggregate((a, b) => a + b);
        public static readonly string EmptyQueueValue = "__________";
        public static readonly int MinAutoColumnSeparationCount = 10;

        public static MessageBuilder AddDefaultQueueMarkup(this MessageBuilder builder, List<User> usersQueue, ViewType view)
        {
            if (usersQueue is null || !usersQueue.Any())
                throw new ArgumentNullException(nameof(usersQueue));

            switch (view)
            {
                case ViewType.Auto: SetAutoQueueMarkup(builder, usersQueue); break;
                case ViewType.Table: SetTableQueueMarkup(builder, usersQueue); break;
                case ViewType.Column: SetColumnQueueMarkup(builder, usersQueue); break;
            }

            return builder;
        }

        private static MessageBuilder SetColumnQueueMarkup(MessageBuilder builder, List<User> usersQueue)
        {
            for (int i = 0; i < usersQueue.Count; i++)
            {
                var user = usersQueue[i];
                if (user is null)
                {
                    builder.AddButtonNextRow(
                        $"{i}. {EmptyQueueValue}",
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


        private static MessageBuilder SetTableQueueMarkup(MessageBuilder builder, List<User> usersQueue, int columns = 2)
        {
            int rows = (int)Math.Ceiling((double)usersQueue.Count / columns);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    int index = i + j * rows;

                    string btnText = "‎‎ ", btnCallback = "_";
                    if (index < usersQueue.Count)
                    {
                        var user = usersQueue[index];
                        if (user is null)
                        {
                            btnText = $"{index}. {EmptyQueueValue}";
                            btnCallback = $"{Actions.Enqueue}{index}";
                        }
                        else
                        {
                            btnText = $"{index}. {user.UserName}";
                            btnCallback = $"{Actions.Dequeue}{user.TelegramId}";
                        }
                    }

                    if (j == columns - 1) builder.AddButtonNextRow(btnText, btnCallback);
                    else builder.AddButton(btnText, btnCallback);
                }
            }

            return builder;
        }

        private static MessageBuilder SetAutoQueueMarkup(MessageBuilder builder, List<User> usersQueue)
        {
            if (usersQueue == null) throw new ArgumentNullException(nameof(usersQueue));
            if(usersQueue.Count > MinAutoColumnSeparationCount)
            {
                SetTableQueueMarkup(builder, usersQueue);
            } else
            {
                SetColumnQueueMarkup(builder, usersQueue);
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
