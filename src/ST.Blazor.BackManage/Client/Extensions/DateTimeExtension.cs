namespace System.Application
{
    public static class DateTimeExtension
    {
        private const int Second = 1;
        private const int Minute = 60 * Second;
        private const int Hour = 60 * Minute;
        private const int Day = 24 * Hour;
        private const int Month = 30 * Day;

        // todo: Need to be localized
        public static string ToFriendlyDisplay(this DateTime dateTime)
        {
            var ts = DateTime.Now - dateTime;
            var delta = ts.TotalSeconds;
            if (delta < 0)
            {
                return "not yet";
            }
            if (delta < 1 * Minute)
            {
                return ts.Seconds == 1 ? "1 second ago" : ts.Seconds + " seconds ago";
            }
            if (delta < 2 * Minute)
            {
                return "1 minute ago";
            }
            if (delta < 45 * Minute)
            {
                return ts.Minutes + "minute";
            }
            if (delta < 90 * Minute)
            {
                return "1 hour ago";
            }
            if (delta < 24 * Hour)
            {
                return ts.Hours + " hours ago";
            }
            if (delta < 48 * Hour)
            {
                return "yesterday";
            }
            if (delta < 30 * Day)
            {
                return ts.Days + " days ago";
            }
            if (delta < 12 * Month)
            {
                var months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "A month ago" : months + " months ago";
            }
            else
            {
                var years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                return years <= 1 ? "a year ago" : years + " years ago";
            }
        }
    }
}