using Newtonsoft.Json;

namespace YouTrackSharp.Projects
{
	public class GenericParameter
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "value")]
		public string Value { get; set; }
	}
}