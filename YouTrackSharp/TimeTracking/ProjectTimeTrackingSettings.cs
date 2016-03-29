using Newtonsoft.Json;

namespace YouTrackSharp.TimeTracking
{
	public class ProjectTimeTrackingSettings
	{
		[JsonProperty(PropertyName = "enabled")]
		public bool Enabled { get; set; }

		[JsonProperty(PropertyName = "estimation")]
		public CustomFieldReference EstimationFieldName { get; set; }

		[JsonProperty(PropertyName = "spentTime")]
		public CustomFieldReference SpentTimeFieldName { get; set; }
	}
}