using Newtonsoft.Json;

using YouTrackSharp.Infrastructure;

namespace YouTrackSharp.Projects
{
	public class WorkType
	{
		#region Public Properties

		[JsonDeserializeOnly]
		public bool AutoAttached { get; set; }

		[JsonDeserializeOnly]
		public string Id { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonDeserializeOnly]
		public string Url { get; set; }

		#endregion

		#region Public Methods and Operators

		/// <summary>
		///     Performs an implicit conversion from <see cref="WorkType" /> to <see cref="System.String" />.
		/// </summary>
		/// <param name="wt">The wt.</param>
		/// <returns>
		///     The result of the conversion.
		/// </returns>
		public static implicit operator string(WorkType wt)
		{
			return wt != null ? wt.Name : default(string);
		}

		/// <summary>
		///     Performs an implicit conversion from <see cref="System.String" /> to <see cref="WorkType" />.
		/// </summary>
		/// <param name="s">The s.</param>
		/// <returns>
		///     The result of the conversion.
		/// </returns>
		public static implicit operator WorkType(string s)
		{
			return new WorkType { Name = s };
		}

		#endregion
	}
}