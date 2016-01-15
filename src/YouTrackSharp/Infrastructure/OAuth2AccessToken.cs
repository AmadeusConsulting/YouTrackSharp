using System;

using RestSharp.Deserializers;

namespace YouTrackSharp.Infrastructure
{
	public class OAuth2AccessToken
	{
		[DeserializeAs(Name="access_token")]
		public string AccessToken { get; set; }
		[DeserializeAs(Name="token_type")]
		public string TokenType { get; set; }
		[DeserializeAs(Name="expires_in")]
		public int ExpiresIn { get; set; }
		public string Scope { get; set; }
		public DateTime? DateObtained { get; set; }

		public bool IsExpired
		{
			get
			{
				if (!DateObtained.HasValue)
				{
					return true;
				}

				return DateObtained.Value.AddSeconds(ExpiresIn) <= DateTime.Now;
			}
		}
	}
}