namespace YouTrackSharp.Infrastructure
{
	/// <summary>
	/// A Thread-Safe implementation of the Credential Store
	/// </summary>
	/// <seealso cref="YouTrackSharp.Infrastructure.ICredentialStore" />
	public class ThreadSafeCredentialStore : ICredentialStore
	{
		private readonly object _credentialSync = new object();
		private OAuth2AccessToken _credential;

		/// <summary>
		/// Gets or sets the credential.
		/// </summary>
		/// <value>
		/// The credential.
		/// </value>
		public OAuth2AccessToken Credential
		{
			get
			{
				lock (_credentialSync)
				{
					return _credential;
				}
			}
			set
			{
				lock (_credentialSync)
				{
					_credential = value;	
				}
			}
		}
	}
}