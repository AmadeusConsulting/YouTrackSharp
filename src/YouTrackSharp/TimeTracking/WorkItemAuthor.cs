using Newtonsoft.Json;

namespace YouTrackSharp.TimeTracking
{
	/// <summary>
	/// </summary>
	public class WorkItemAuthor
	{
		[JsonProperty(PropertyName = "login")]
		public string Login { get; set; }

		[JsonProperty(PropertyName = "ringId")]
		public string RingId { get; set; }

		[JsonProperty(PropertyName = "url")]
		public string Url { get; set; }
	}
}