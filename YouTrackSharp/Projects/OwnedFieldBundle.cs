using System.Collections.Generic;

using Newtonsoft.Json;

namespace YouTrackSharp.Projects
{
	public class OwnedFieldBundle
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "ownedField")]
		public List<OwnedField> OwnedFields { get; set; } 
	}
}
