using System;

using Newtonsoft.Json;

namespace YouTrackSharp.Infrastructure
{
	/// <summary>
	///     Converts <see cref="DateTimeOffset" /> values to/from epoch timestamp values
	/// </summary>
	/// <seealso cref="Newtonsoft.Json.JsonConverter" />
	public class TimestampSecondsJsonConverter : JsonConverter
	{
		#region Fields

		private DateTimeOffset _epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);

		#endregion

		#region Public Methods and Operators

		/// <summary>
		///     Determines whether this instance can convert the specified object type.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		/// <returns>
		///     <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
		/// </returns>
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(DateTimeOffset);
		}

		/// <summary>
		///     Reads the JSON representation of the object.
		/// </summary>
		/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
		/// <param name="objectType">Type of the object.</param>
		/// <param name="existingValue">The existing value of object being read.</param>
		/// <param name="serializer">The calling serializer.</param>
		/// <returns>
		///     The object value.
		/// </returns>
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var value = reader.Value.ToString();
			var t = long.Parse(value);
			return _epoch.AddMilliseconds(t);
		}

		/// <summary>
		///     Writes the JSON representation of the object.
		/// </summary>
		/// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
		/// <param name="value">The value.</param>
		/// <param name="serializer">The calling serializer.</param>
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var dateTime = (DateTimeOffset)value;

			long secondsSinceEpoch = (long)(dateTime - _epoch).TotalMilliseconds;

			writer.WriteValue(secondsSinceEpoch);
		}

		#endregion
	}
}