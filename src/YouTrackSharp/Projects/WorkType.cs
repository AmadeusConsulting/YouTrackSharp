using Newtonsoft.Json;

using YouTrackSharp.Infrastructure;

namespace YouTrackSharp.Projects
{
	public class WorkType
	{
		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonDeserializeOnly]
		public string Id { get; set; }
		[JsonDeserializeOnly]
		public bool AutoAttached { get; set; }
		[JsonDeserializeOnly]
		public string Url { get; set; }
	}
}