using System;

namespace ForgetMeNot.Common
{
	public static class TimeExtensions
	{
		public static TimeSpan Milliseconds(this int millis)
		{
			return TimeSpan.FromMilliseconds (millis);
		}

        /* These are already defined in the FluentAssertions assembly
		public static TimeSpan Seconds(this int seconds)
		{
			return TimeSpan.FromSeconds (seconds);
		}

		public static TimeSpan Minutes(this int minutes)
		{
			return TimeSpan.FromMinutes (minutes);
		}

		public static TimeSpan Hours(this int hours)
		{
			return TimeSpan.FromHours (hours);
		}

		public static TimeSpan Days(this int days)
		{
			return TimeSpan.FromDays (days);
		}*/
	}
}

