using System.Collections.Generic;

using Newtonsoft.Json;

using RestSharp.Deserializers;
using RestSharp.Serializers;

namespace YouTrackSharp.Projects
{
	[SerializeAs(Name = "ownedFieldBundle")]
	public class OwnedFieldBundle
	{
		[JsonProperty(PropertyName = "name")]
		[DeserializeAs(Attribute = true, Name = "name")]
		[SerializeAs(Attribute = true, Name = "name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "ownedField")]
		public List<OwnedField> OwnedFields { get; set; } 
	}
}
