using System;

namespace TryScanMe.Functions.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToFriendlyDate(this DateTime date)
        {
            var time = (date - DateTime.UtcNow).Duration();

            if (time.TotalSeconds < 5)
            {
                return "Just now";
            }
            if (time.TotalSeconds >= 5 && time.TotalSeconds < 60)
            {
                return Convert.ToInt32(time.Seconds).ToString() + " " + time.Seconds.Ago().Insert(0, "second");
            }
            else if (time.TotalSeconds >= 60 && time.TotalMinutes < 60)
            {
                return Convert.ToInt32(time.TotalMinutes).ToString() + " " + time.TotalMinutes.Ago().Insert(0, "minute");
            }
            else if (time.TotalMinutes >= 60 && time.TotalHours< 24)
            {
                return Convert.ToInt32(time.TotalHours).ToString() + " " + time.TotalHours.Ago().Insert(0, "hour");
            }
            else if (time.TotalHours >= 24 && time.TotalDays < 7)
            {
                return Convert.ToInt32(time.TotalDays).ToString() + " " + time.TotalDays.Ago().Insert(0, "day");
            }
            else if (time.TotalDays >= 7 && time.TotalDays < 28)
            {
                return Convert.ToInt32(time.TotalDays / 7).ToString() + " " + (time.TotalDays / 7).Ago().Insert(0, "week");
            }
            else if (time.TotalDays >= 28 && time.TotalDays < 365)
            {
                return Convert.ToInt32(time.TotalDays/ 30).ToString() + " " + (time.TotalDays / 30).Ago().Insert(0, "month"); //There aren't 30 days in every month
            }
            else if (time.TotalDays >= 365)
            {
                return Convert.ToInt32(time.TotalDays / 365).ToString() + " " + (time.TotalDays / 365).Ago().Insert(0, "year");
            }

            return "sometime ago";
        }
        
        private static string Ago(this double time)
        {
            return Math.Round(time) == 1
                ? " ago"
                : "s ago";
        }

        private static string Ago(this int time)
        {
            return time == 1
                ? " ago"
                : "s ago";
        }
    }
}
