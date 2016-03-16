using System;
using System.Net;

using RestSharp;
using RestSharp.Authenticators;

namespace YouTrackSharp.Infrastructure
{
	public interface IRestClientFactory
	{
		IRestClient BuildRestClient(Uri baseUri, IAuthenticator authenticator);
	}
}