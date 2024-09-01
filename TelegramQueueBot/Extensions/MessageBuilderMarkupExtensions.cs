using Hangfire.Common;
using System.Globalization;
using TelegramQueueBot.Common;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Models;
using TelegramQueueBot.Models.Enums;
using TelegramQueueBot.Repository.Interfaces;

namespace TelegramQueueBot.Extensions
{
    public static class MessageBuilderMarkupExtensions
    {
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

        public static MessageBuilder AddEmptyQueueMarkup(this MessageBuilder builder, int queueSize, ViewType view)
        {
            if (queueSize < 2 || queueSize > 100) throw new ArgumentOutOfRangeException(nameof(queueSize), "Range for queue size param is [2;100]");

            var list = new List<User>(new User[queueSize]);
            builder.AddDefaultQueueMarkup(list, view);
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


        private static MessageBuilder SetTableQueueMarkup(MessageBuilder builder, List<User> usersQueue, int columns = 2, bool emptyCallback = false)
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
                            if (!emptyCallback) btnCallback = $"{Actions.Enqueue}{index}";
                        }
                        else
                        {
                            btnText = $"{index}. {user.UserName}";
                            if (!emptyCallback) btnCallback = $"{Actions.Dequeue}{user.TelegramId}";
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
            if (usersQueue.Count > MinAutoColumnSeparationCount)
            {
                SetTableQueueMarkup(builder, usersQueue);
            }
            else
            {
                SetColumnQueueMarkup(builder, usersQueue);
            }
            return builder;
        }

        public static MessageBuilder AddSavedQueueMarkup(this MessageBuilder builder, List<User> usersQueue)
        {
            if (usersQueue is null || !usersQueue.Any())
                throw new ArgumentNullException(nameof(usersQueue));

            SetTableQueueMarkup(builder, usersQueue, 2, true);
            return builder;
        }

        public static async Task<MessageBuilder> AddJobMenuMarkup(this MessageBuilder builder, string jobId, DateTime? nextOccurrence, ITextRepository textRepository)
        {
            var culture = new CultureInfo("uk-UA");
            builder
                .AddButton("_", "_")
                .AddButton("_", "_")
                .AddButtonNextRow(await textRepository.GetValueAsync(TextKeys.DeleteQueueBtn), $"{Actions.DeleteJob}{jobId}")

                .AddButton("-60", $"{Actions.AddMinutes}{-60}/{jobId}")
                .AddButton("-15", $"{Actions.AddMinutes}{-15}/{jobId}")
                .AddButton("-5", $"{Actions.AddMinutes}{-5}/{jobId}")
                .AddButton(nextOccurrence.Value.ToLocalTime().ToString("HH:mm"), "_")
                .AddButton("5", $"{Actions.AddMinutes}{5}/{jobId}")
                .AddButton("15", $"{Actions.AddMinutes}{15}/{jobId}")
                .AddButtonNextRow("60", $"{Actions.AddMinutes}{60}/{jobId}")

                .AddButton("◀️", $"{Actions.AddDays}{-1}/{jobId}")
                .AddButton(culture.DateTimeFormat.GetDayName(nextOccurrence.Value.ToLocalTime().DayOfWeek), "_")
                .AddButtonNextRow("▶️", $"{Actions.AddDays}{1}/{jobId}");
            return builder;
        }

        public static async Task<MessageBuilder> AddJobMenuCaption(this MessageBuilder builder, ChatJob job, ITextRepository _textRepository)
        {
            builder
                .AppendText(await _textRepository.GetValueAsync(TextKeys.JobMenu)).AppendTextLine(job.JobName)
                .AppendText("[DEBUG last] ").AppendTextLine(job.LastRunTime.ToLocalTime().ToString())
                .AppendText("[DEBUG next] ").AppendTextLine(job.NextRunTime.ToLocalTime().ToString())
                .AppendText(await _textRepository.GetValueAsync(TextKeys.JobNextTime)).AppendTextLine(job.NextRunTime.ToLocalTime().ToString("dd.MM.yyyy"));

            return builder;
        }
    }
}
