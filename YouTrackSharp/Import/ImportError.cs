using Newtonsoft.Json;

namespace YouTrackSharp.Import
{
	//{"error":{"type":"UNKNOWN_VALUE","fieldName":"author.login","value":"Value is unknown","format":null}
	public class ImportError
	{
		[JsonProperty(PropertyName = "error")]
		public ImportErrorDetail Detail { get; set; }
	}
}