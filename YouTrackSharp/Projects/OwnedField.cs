using Newtonsoft.Json;

using RestSharp.Deserializers;
using RestSharp.Serializers;

namespace YouTrackSharp.Projects
{
	[SerializeAs(Name = "ownedField")]
	public class OwnedField
	{
		[JsonProperty(PropertyName = "value")]
		[DeserializeAs(Name = "value")]
		[SerializeAs(Name = "value", Attribute = false)]
		public string Value { get; set; }

		[JsonProperty(PropertyName = "description")]
		[DeserializeAs(Name = "description")]
		[SerializeAs(Name = "description", Attribute = true)]
		public string Description { get; set; }

		[JsonProperty(PropertyName = "owner")]
		[DeserializeAs(Name = "owner")]
		[SerializeAs(Name = "owner", Attribute = true)]
		public string Owner { get; set; }

		[JsonProperty(PropertyName = "colorIndex")]
		[DeserializeAs(Name = "colorIndex")]
		[SerializeAs(Name = "colorIndex", Attribute = true)]
		public string ColorIndex { get; set; }
	}
}