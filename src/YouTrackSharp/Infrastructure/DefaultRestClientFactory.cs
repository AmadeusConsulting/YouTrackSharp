using System;
using System.Net;

using RestSharp;
using RestSharp.Authenticators;

namespace YouTrackSharp.Infrastructure
{
	public class DefaultRestClientFactory : IRestClientFactory
	{
		public IRestClient BuildRestClient(Uri baseUri, IAuthenticator authenticator)
		{
			return new RestClient { BaseUrl = baseUri, Authenticator = authenticator };
		}
	}
}