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
	}
}
