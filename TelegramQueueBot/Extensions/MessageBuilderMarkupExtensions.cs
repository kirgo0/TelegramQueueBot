using System.Globalization;
using TelegramQueueBot.Common;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Models;
using TelegramQueueBot.Models.Enums;

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

        public static MessageBuilder AppendModeTitle(this MessageBuilder builder, Chat chat)
        {
            if (chat.Mode is ChatMode.CallingUsers)
            {
                builder.AppendTextLine(TextResources.GetValue(TextKeys.QueueIsCallingUsers));
                builder.AppendTextLine();
            }
            return builder;
        }

        public static MessageBuilder AddEmptyQueueMarkup(this MessageBuilder builder, int queueSize, ViewType view)
        {
            if (queueSize < 2 || queueSize > 100) throw new ArgumentOutOfRangeException(nameof(queueSize), "Size must be in range [2;100]");

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
                        callbackData: $"{Common.Action.Enqueue}{i}"
                        );
                }
                else
                {
                    builder.AddButtonNextRow(
                        $"{i}. {user.UserName}",
                        callbackData: $"{Common.Action.Dequeue}{user.TelegramId}"
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
                            if (!emptyCallback) btnCallback = $"{Common.Action.Enqueue}{index}";
                        }
                        else
                        {
                            btnText = $"{index}. {user.UserName}";
                            if (!emptyCallback) btnCallback = $"{Common.Action.Dequeue}{user.TelegramId}";
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

        public static MessageBuilder AddJobMenuCaption(this MessageBuilder builder, ChatJob job)
        {
            builder
                .AppendText(TextResources.GetValue(TextKeys.JobMenu)).AppendTextLine(job.JobName)
                .AppendText(TextResources.GetValue(TextKeys.JobNextTime)).AppendTextLine(job.NextRunTimeUtc.ToKyivTime().ToString("dd.MM.yyyy"));

            return builder;
        }

        public static MessageBuilder AddJobMenuMarkup(this MessageBuilder builder, ChatJob job, string? queueName = null)
        {

            builder
                .AddButton(TextResources.GetValue(TextKeys.BackBtn), Common.Action.Jobs)
                .AddButton($"{(string.IsNullOrEmpty(queueName) ? TextResources.GetValue(TextKeys.LoadJobWithQueueBtn) : queueName)}", $"{Common.Action.JobQueueMenu}{job.Id}")
                .AddButtonNextRow(TextResources.GetValue(TextKeys.DeleteQueueBtn), $"{Common.Action.DeleteJob}{job.Id}")

                .AddJobMenuMinutes(job, 5, 15, 60)

                .AddJobMenuWeeks(job, 1)

                .AddJobMenuIntervals(job, 4);


            return builder;
        }

        private static MessageBuilder AddJobMenuMinutes(this MessageBuilder builder, ChatJob job, params int[] minutes)
        {
            for (var i = minutes.Length - 1; i >= 0; i--)
            {
                builder.AddButton($"-{minutes[i]}", $"{Common.Action.AddMinutes}{-minutes[i]}/{job.Id}");
            }
            builder.AddButton(job.NextRunTimeUtc.ToKyivTime().ToString("HH:mm"), "_");
            for (var i = 0; i < minutes.Length - 1; i++)
            {

                builder.AddButton($"{minutes[i]}", $"{Common.Action.AddMinutes}{minutes[i]}/{job.Id}");
            }
            builder.AddButtonNextRow(
                $"{minutes[minutes.Length - 1]}",
                $"{Common.Action.AddMinutes}{minutes[minutes.Length - 1]}/{job.Id}"
                );
            return builder;
        }

        private static MessageBuilder AddJobMenuIntervals(this MessageBuilder builder, ChatJob job, int maxInterval)
        {
            builder
                .AddButton(TextResources.GetValue(TextKeys.MoveLeftBtn), $"{Common.Action.AddIntervalWeeks}{1}/{job.Id}")
                .AddButton(TextResources.GetValue(TextKeys.SetInterval), "_")
                .AddButtonNextRow(TextResources.GetValue(TextKeys.MoveRightBtn), $"{Common.Action.AddIntervalWeeks}{-1}/{job.Id}");

            for (int i = 1; i <= maxInterval; i++)
            {
                builder.AddButton(
                    i == job.Interval ? $"{i} {TextResources.GetValue(TextKeys.SelectedBtn)}" : i.ToString(),
                    $"{Common.Action.SetInterval}{i}/{job.Id}"
                    );
            }

            return builder;
        }

        private static MessageBuilder AddJobMenuWeeks(this MessageBuilder builder, ChatJob job, int shift)
        {
            var culture = new CultureInfo("uk-UA");
            return builder
                .AddButton(TextResources.GetValue(TextKeys.MoveLeftBtn), $"{Common.Action.AddDays}{-shift}/{job.Id}")
                .AddButton(culture.DateTimeFormat.GetDayName(job.NextRunTimeUtc.ToKyivTime().DayOfWeek), "_")
                .AddButtonNextRow(TextResources.GetValue(TextKeys.MoveRightBtn), $"{Common.Action.AddDays}{shift}/{job.Id}");
        }


    }
}
