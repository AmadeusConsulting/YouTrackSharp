using Newtonsoft.Json;

namespace YouTrackSharp.Issues
{
	public class Tag
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "visibleForGroup")]
		public string VisibleForGroup { get; set; }

		[JsonProperty(PropertyName = "updatableByGroup")]
		public string UpdatableByGroup { get; set; }

		[JsonProperty(PropertyName = "untagOnResolve")]
		public bool UntagOnResolve { get; set; }
	}
}