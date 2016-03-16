using Newtonsoft.Json;

using YouTrackSharp.Infrastructure;

namespace YouTrackSharp.TimeTracking
{
	/// <summary>
	/// </summary>
	public class WorkItemAuthor
	{
		[JsonProperty(PropertyName = "login")]
		public string Login { get; set; }

		[JsonProperty(PropertyName = "ringId")]
		[JsonDeserializeOnly]
		public string RingId { get; set; }

		[JsonProperty(PropertyName = "url")]
		[JsonDeserializeOnly]
		public string Url { get; set; }
	}
}