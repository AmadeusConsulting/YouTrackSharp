using Newtonsoft.Json;

namespace YouTrackSharp.Projects
{
	public class OwnedField
	{
		[JsonProperty(PropertyName = "value")]
		public string Value { get; set; }
		[JsonProperty(PropertyName = "description")]
		public string Description { get; set; }
		[JsonProperty(PropertyName = "owner")]
		public string Owner { get; set; }
		[JsonProperty(PropertyName = "colorIndex")]
		public string ColorIndex { get; set; }
	}
}