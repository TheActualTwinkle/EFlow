namespace EFlow.Common.Extensions;

public static class TimeSpanExtensions
{
    extension(TimeSpan timeSpan)
    {
        public string ToCronExpression()
        {
            if (timeSpan <= TimeSpan.Zero || timeSpan.TotalSeconds < 1)
                throw new ArgumentException($"TimeSpan must be greater than zero seconds but was {timeSpan}");

            var seconds = timeSpan.Seconds;
            var minutes = timeSpan.Minutes;
            var hours = timeSpan.Hours;

            if (timeSpan.Days > 0)
                return $"{seconds} {minutes} {hours} {(timeSpan.Days == 1 ? "*" : $"*/{timeSpan.Days}")} * *";

            if (hours > 0)
                return $"{seconds} {minutes} {(hours == 1 ? "*" : $"*/{hours}")} * * *";

            if (minutes > 0)
                return $"{seconds} {(minutes == 1 ? "*" : $"*/{minutes}")} * * * *";

            if (seconds > 0)
                return $"0/{seconds} * * * * *";

            throw new ArgumentException($"Unable to convert TimeSpan {timeSpan} to cron expression.");
        }
    }
}
