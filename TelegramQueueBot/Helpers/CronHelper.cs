using Cronos;

namespace TelegramQueueBot.Helpers
{
    public class CronHelper
    {

        public static string AddMinutes(string cronExpression, int minutesToAdd)
        {
            var parts = cronExpression.Split(' ');

            if (parts.Length != 5)
                throw new ArgumentException("Invalid cron expression. It should have 5 parts.");

            var nextOccurence = CronExpression.Parse(cronExpression).GetNextOccurrence(DateTime.UtcNow);
            if (!nextOccurence.HasValue)
            {
                throw new Exception();
            }

            var timeToOccure = nextOccurence.Value.ToLocalTime().AddMinutes(minutesToAdd).ToUniversalTime();

            int minute = timeToOccure.Minute;
            int hour = timeToOccure.Hour;
            int dayOfWeek = (int)timeToOccure.DayOfWeek;

            return $"{minute} {hour} {parts[2]} {parts[3]} {dayOfWeek}";
        }

        public static string AddDays(string cronExpression, int daysToAdd)
        {
            var parts = cronExpression.Split(' ');

            if (parts.Length != 5)
                throw new ArgumentException("Invalid cron expression. It should have 5 parts.");

            int.TryParse(parts[4], out int day);

            DayOfWeek newDay = (DayOfWeek)((((day + daysToAdd) % 7) + 7) % 7);

            string newCronExpression = $"{string.Join(" ", parts.Take(4))} {(int)newDay}";

            return newCronExpression;
        }
    }
}
