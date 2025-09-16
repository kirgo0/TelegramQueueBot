using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramQueueBot.Extensions
{
    using System;

    public static class DateTimeExtensions
    {
        private static readonly TimeZoneInfo KyivZone =
            OperatingSystem.IsWindows() ?
                TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time") :
                TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");

        /// <summary>
        /// Converts a UTC DateTime to Kyiv local time.
        /// </summary>
        public static DateTime ToKyivTime(this DateTime utcDateTime)
        {
            if (utcDateTime.Kind != DateTimeKind.Utc)
                throw new ArgumentException("Input DateTime must be in UTC", nameof(utcDateTime));

            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, KyivZone);
        }

        /// <summary>
        /// Converts a UTC DateTime to Kyiv time and formats as HH:mm.
        /// </summary>
        public static string ToKyivTimeString(this DateTime utcDateTime, string format = "HH:mm")
        {
            return utcDateTime.ToKyivTime().ToString(format);
        }
    }

}
