using Newtonsoft.Json;

using YouTrackSharp.Infrastructure;

namespace YouTrackSharp.TimeTracking
{
	public class CustomFieldReference
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		[JsonDeserializeOnly]
		public string Url { get; set; }
	}
}