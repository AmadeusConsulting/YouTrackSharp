namespace YouTrackSharp.Infrastructure
{
	public interface ICredentialStore
	{
		OAuth2AccessToken Credential { get; set; }
	}
}