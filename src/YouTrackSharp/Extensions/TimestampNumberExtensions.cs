using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YouTrackSharp.Extensions
{
	public static class TimestampNumberExtensions
	{
		/// <summary>
		/// Converts the current <see langword="long"/> value into a <see cref="DateTimeOffset" />
		/// value, assuming the current <see langword="long"/> value is a timestamp in milliseconds.
		/// </summary>
		/// <param name="number">The number.</param>
		/// <returns>
		/// The equivalent DateTimeOffset value for the current <see langword="long"/> value
		/// </returns>
		public static DateTimeOffset DateTimeOffsetFromTimestamp(this long number)
		{
			var epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
			return epoch.AddMilliseconds(number);
		}
	}
}
