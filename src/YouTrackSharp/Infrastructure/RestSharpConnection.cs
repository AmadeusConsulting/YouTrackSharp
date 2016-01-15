using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;

using log4net;

using Microsoft.Win32;

using RestSharp;
using RestSharp.Authenticators;

using YouTrackSharp.Admin;
using YouTrackSharp.Issues;
using YouTrackSharp.Projects;

namespace YouTrackSharp.Infrastructure
{
	public class RestSharpConnection : IConnection
	{
		#region Constants

		private const string OAuth2TokenResource = "hub/api/rest/oauth2/token";

		private const string YouTrackRestResourceBase = "youtrack/rest";

		#endregion

		#region Fields

		private readonly object _authenticationSync = new object();

		private readonly Uri _baseUri;

		private readonly string _clientId;

		private readonly string _clientSecret;

		private readonly ILog _log = LogManager.GetLogger(typeof(RestSharpConnection));

		private readonly IRestClientFactory _restClientFactory;

		private readonly string _youtrackScopeId;

		private OAuth2AccessToken _accessToken;

		#endregion

		#region Constructors and Destructors

		public RestSharpConnection(Uri apiBaseUrl, IRestClientFactory restClientFactory, string clientId, string clientSecret, string youtrackScopeId)
		{
			if (apiBaseUrl == null)
			{
				throw new ArgumentNullException("apiBaseUrl");
			}
			if (restClientFactory == null)
			{
				throw new ArgumentNullException("restClientFactory");
			}
			if (string.IsNullOrEmpty(youtrackScopeId))
			{
				throw new ArgumentNullException("youtrackScopeId");
			}
			if (string.IsNullOrEmpty(clientSecret))
			{
				throw new ArgumentNullException("clientSecret");
			}
			if (string.IsNullOrEmpty(clientId))
			{
				throw new ArgumentNullException("clientId");
			}

			_baseUri = apiBaseUrl;
			_restClientFactory = restClientFactory;
			_clientId = clientId;
			_clientSecret = clientSecret;
			_youtrackScopeId = youtrackScopeId;
		}

		#endregion

		#region Public Properties

		public HttpStatusCode HttpStatusCode { get; private set; }

		public bool IsAuthenticated
		{
			get
			{
				if (_accessToken == null)
				{
					return false;
				}
				return !_accessToken.IsExpired;
			}
		}

		#endregion

		#region Public Methods and Operators

		public void Authenticate(string username, string password)
		{
			// Obtains a new OAuth2 Access Token using the Resource Owner Password workflow
			// see https://www.jetbrains.com/hub/help/1.0/Resource-Owner-Password-Credentials.html

			var request = new RestRequest(OAuth2TokenResource, Method.POST);
			request.AddParameter("username", username);
			request.AddParameter("password", password);
			request.AddParameter("grant_type", "password");
			request.AddParameter("scope", _youtrackScopeId);

			var client = GetClient(skipAuthentication: true);
			client.Authenticator = new HttpBasicAuthenticator(_clientId, _clientSecret);

			var response = client.Execute<OAuth2AccessToken>(request);
			DateTime dateObtained = DateTime.Now;

			try
			{
				EnsureExpectedResponseStatus(response, HttpStatusCode.OK);
			}
			catch (ConnectionException ex)
			{
				throw new AuthenticationException(Language.YouTrackClient_Login_Authentication_Failed, ex);
			}

			lock (_authenticationSync)
			{
				_accessToken = response.Data;
				_accessToken.DateObtained = dateObtained;
			}
		}

		public void Delete(string command)
		{
			EnsureAuthenticated();

			var request = new RestRequest(string.Format("{0}/{1}", YouTrackRestResourceBase, command), Method.DELETE);
			var response = GetClient().Execute(request);

			HttpStatusCode = response.StatusCode;

			if (response.StatusCode != HttpStatusCode.OK)
			{
				throw new InvalidRequestException(string.Format("Request Failed! \n{0}", response.Content));
			}
		}

		public T Get<T>(string command) where T : new()
		{
			EnsureAuthenticated();

			var request = new RestRequest(string.Format("{0}/{1}", YouTrackRestResourceBase, command), Method.GET);

			IRestResponse<T> response = GetClient().Execute<T>(request);
			HttpStatusCode = response.StatusCode;

			if (response.ResponseStatus != ResponseStatus.Completed)
			{
				throw response.ErrorException;
			}

			if (response.StatusCode == HttpStatusCode.OK)
			{
				return response.Data;
			}
			else
			{
				throw new InvalidRequestException(string.Format("Request Failed! \n{0}", response.Content));
			}
		}

		public IEnumerable<TInternal> Get<TWrapper, TInternal>(string command) where TWrapper : class, IDataWrapper<TInternal>, new()
			where TInternal : new()
		{
			EnsureAuthenticated();

			// TInternal data = GetWrappedData<TInternal, TWrapper>(command); 

			var request = new RestRequest(string.Format("{0}/{1}", YouTrackRestResourceBase, command), Method.GET) { RequestFormat = DataFormat.Json };
			var response = GetClient().Execute<TWrapper>(request);

			return response.Data.Data ?? new TInternal[0];
		}

		public User GetCurrentAuthenticatedUser()
		{
			EnsureAuthenticated();

			var user = Get<User>(string.Format("{0}/{1}", YouTrackRestResourceBase, "user/current"));

			return user;
		}

		public IEnumerable<T> GetList<T>(string command) where T : new()
		{
			EnsureAuthenticated();

			var request = new RestRequest(string.Format("{0}/{1}", YouTrackRestResourceBase, command), Method.GET);

			IRestResponse<List<T>> response = GetClient().Execute<List<T>>(request);
			HttpStatusCode = response.StatusCode;

			if (response.ResponseStatus != ResponseStatus.Completed)
			{
				throw response.ErrorException;
			}

			if (response.StatusCode == HttpStatusCode.OK)
			{
				return response.Data;
			}
			else
			{
				throw new InvalidRequestException(string.Format("Request Failed! \n{0}", response.Content));
			}
		}

		public TInternal GetWrappedData<TInternal, TWrapper>(string command) where TWrapper : class, IDataWrapper<TInternal>, new()
			// where TInternal : class
		{
			EnsureAuthenticated();

			var request = new RestRequest(string.Format("{0}/{1}", YouTrackRestResourceBase, command), Method.GET);
			var response = GetClient().Execute<TWrapper>(request);

			return response.Data.Data != null ? response.Data.Data.First() : default(TInternal);
		}

		public void Head(string command)
		{
			EnsureAuthenticated();

			var request = new RestRequest(string.Format("{0}/{1}", YouTrackRestResourceBase, command), Method.HEAD);

			var response = GetClient().Execute(request);
			HttpStatusCode = response.StatusCode;
		}

		public void Logout()
		{
			_accessToken = null;
		}

		public dynamic Post(string command, object data, string accept)
		{
			EnsureAuthenticated();

			var expando = (IDictionary<string, object>)data;
			var request =
				new RestRequest(
					string.Format("{0}/{1}", YouTrackRestResourceBase, "issue?project={project}&summary={summary}&description={description}"),
					Method.POST) { RequestFormat = DataFormat.Json };
			request.AddUrlSegment("project", expando.FirstOrDefault(x => x.Key == "project").Value.ToString());
			request.AddUrlSegment("summary", expando.FirstOrDefault(x => x.Key == "summary").Value.ToString());
			request.AddUrlSegment("description", expando.FirstOrDefault(x => x.Key == "description").Value.ToString());
			request.AddHeader("accept", accept);
			foreach (KeyValuePair<string, object> kvp in expando)
			{
				request.AddParameter(kvp.Key, kvp.Value);
			}

			var response = GetClient().Execute<ListIssue>(request);
			return response.Data;
		}

		public void Post(string command, object data)
		{
			EnsureAuthenticated();

			var expando = (IDictionary<string, object>)data;
			var request = new RestRequest(string.Format("{0}/{1}", YouTrackRestResourceBase, command), Method.POST);
			foreach (KeyValuePair<string, object> kvp in expando)
			{
				request.AddParameter(kvp.Key, kvp.Value);
			}
			var response = GetClient().Execute(request);

			HttpStatusCode = response.StatusCode;

			if (response.ResponseStatus != ResponseStatus.Completed)
			{
				throw response.ErrorException;
			}
		}

		public void PostFile(string command, string path)
		{
			EnsureAuthenticated();

			var contentType = GetFileContentType(path);
			var request = new RestRequest(string.Format("{0}/{1}", YouTrackRestResourceBase, command), Method.POST);
			request.AddFile("file", File.ReadAllBytes(path), Path.GetFileName(path), contentType);
			request.AddHeader("Content-type", "application/json");
			request.AddHeader("Accept", "application/json");
			request.RequestFormat = DataFormat.Json;
			var response = GetClient().Execute(request);

			HttpStatusCode = response.StatusCode;
		}

		public void Put(string command, object data)
		{
			EnsureAuthenticated();

			RestRequest request;
			if (data != null)
			{
				var expando = (IDictionary<string, object>)data;
				request =
					new RestRequest(
						string.Format("{0}/{1}", YouTrackRestResourceBase, "issue?project={project}&summary={summary}&description={description}"),
						Method.PUT);
				request.AddUrlSegment("project", expando.FirstOrDefault(x => x.Key == "project").Value.ToString());
				request.AddUrlSegment("summary", expando.FirstOrDefault(x => x.Key == "summary").Value.ToString());
				request.AddUrlSegment("description", expando.FirstOrDefault(x => x.Key == "description").Value.ToString());
			}
			else
			{
				request = new RestRequest(string.Format("{0}/{1}", YouTrackRestResourceBase, command), Method.PUT);
			}

			var response = GetClient().Execute(request);

			HttpStatusCode = response.StatusCode;

			if (response.ResponseStatus != ResponseStatus.Completed)
			{
				throw response.ErrorException;
			}
		}

		#endregion

		#region Methods

		private void EnsureAuthenticated()
		{
			// in the absence of an explicit call to the Authenticate method, we use the Client Credentials workflow to obtain an access token for YouTrack
			// see https://www.jetbrains.com/hub/help/1.0/Client-Credentials.html
			lock (_authenticationSync)
			{
				if (!IsAuthenticated)
				{
					var client = GetClient(skipAuthentication: true);
					// make a request to the oauth2 token endpoint using the client_credentials grant
					var request = new RestRequest(OAuth2TokenResource, Method.POST);
					request.AddParameter("grant_type", "client_credentials");
					request.AddParameter("scope", _youtrackScopeId);
					client.Authenticator = new HttpBasicAuthenticator(_clientId, _clientSecret);

					var response = client.Execute<OAuth2AccessToken>(request);
					var dateObtained = DateTime.Now;
					EnsureExpectedResponseStatus(response, HttpStatusCode.OK);
					_accessToken = response.Data;
					_accessToken.DateObtained = dateObtained;
				}
			}
		}

		private void EnsureExpectedResponseStatus(IRestResponse response, params HttpStatusCode[] expectedStatuses)
		{
			_log.Debug(string.Format("Checking request status for {0} ...", response.Request.Resource));

			if (response.ResponseStatus != ResponseStatus.Completed)
			{
				_log.Error(string.Format("!!!! Response status = {0} !!!!", response.ResponseStatus), response.ErrorException);
				throw new ConnectionException(
					string.Format("API request did not complete successfully.  ResponseStatus = {0}", response.ResponseStatus),
					response.ErrorException);
			}
			if (!expectedStatuses.Contains(response.StatusCode))
			{
				_log.Warn(string.Format("Response returned an unexpected status = {0}", response.StatusCode));
				throw new ConnectionException(
					string.Format(
						"API request returned an unexpected response: {0} ({1}) -- Expected one of {2} \n\n {3}",
						response.StatusCode,
						response.StatusDescription,
						expectedStatuses.Select(s => s.ToString()).Aggregate((s1, s2) => string.Format("{0}{1}", s1, s2)),
						response.Content));
			}
		}

		private IRestClient GetClient(bool skipAuthentication = false)
		{
			IAuthenticator authenticator = null;

			if (IsAuthenticated && !skipAuthentication)
			{
				authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(_accessToken.AccessToken, _accessToken.TokenType);
			}

			var client = _restClientFactory.BuildRestClient(_baseUri, authenticator);

			client.AddHandler("application/json", new NewtonsoftJsonDeserializer());

			return client;
		}

		private string GetFileContentType(string filename)
		{
			var mime = "application/octetstream";
			var extension = Path.GetExtension(filename);
			if (extension != null)
			{
				var ext = extension.ToLower();
				var rk = Registry.ClassesRoot.OpenSubKey(ext);
				if (rk != null && rk.GetValue("Content Type") != null)
				{
					mime = rk.GetValue("Content Type").ToString();
				}
			}
			return mime;
		}

		#endregion
	}
}