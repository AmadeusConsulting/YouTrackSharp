using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

using YouTrackSharp.Infrastructure;

namespace YouTrackSharp.TimeTracking
{
	public class WorkItem
	{
		[JsonProperty(PropertyName="id")]
		public string Id { get; set; }

		[JsonProperty(PropertyName = "url")]
		public string Url { get; set; }

		[JsonProperty(PropertyName = "date")]
		[JsonConverter(typeof(TimestampSecondsJsonConverter))]
		public DateTimeOffset Date { get; set; }

		/// <summary>
		/// The Duration of the work item, in minutes
		/// </summary>
		[JsonProperty(PropertyName = "duration")]
		public double Duration { get; set; }

		[JsonProperty(PropertyName = "description")]
		public string Description { get; set; }

		[JsonProperty(PropertyName = "author")]
		public WorkItemAuthor Author { get; set; }

		[JsonProperty(PropertyName = "workType")]
		public string WorkType { get; set; }
	}
}
