using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YouTrackSharp.Extensions
{
	public static class DateTimeOffsetExtensions
	{
		/// <summary>
		/// Returns a new <see cref="DateTimeOffset"/> object with the same value as the original, minus the value for Ticks.
		/// </summary>
		/// <param name="date">The date.</param>
		/// <returns></returns>
		public static DateTimeOffset NoTicks(this DateTimeOffset date)
		{
			return new DateTimeOffset(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond, date.Offset);
		}

		/// <summary>
		/// Convert the current DateTimeOffset to an epoch timestamp in milliseconds.
		/// </summary>
		/// <param name="date">The date.</param>
		/// <returns></returns>
		public static long ToTimestampMillis(this DateTimeOffset date)
		{
			var epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
			return (long)(date - epoch).TotalMilliseconds;
		}
	}
}
