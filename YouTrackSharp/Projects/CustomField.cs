using System.Collections.Generic;

using Newtonsoft.Json;

namespace YouTrackSharp.Projects
{
	public class CustomField
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "type")]
		public string Type { get; set; }

		[JsonProperty(PropertyName = "emptyText")]
		public string EmptyText { get; set; }

		[JsonProperty(PropertyName = "canBeEmpty")]
		public bool CanBeEmpty { get; set; }

		[JsonProperty(PropertyName = "param")]
		public List<GenericParameter> Parameters { get; set; }

		[JsonProperty(PropertyName = "defaultValue")]
		public List<string> DefaultValues { get; set; } 
	}
}