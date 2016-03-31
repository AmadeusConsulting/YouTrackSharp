using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;

using log4net;

using Microsoft.Win32;

using RestSharp;
using RestSharp.Authenticators;

using YouTrackSharp.Admin;
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

		private readonly Uri _baseUri;

		private readonly string _clientId;

		private readonly string _clientSecret;

		private readonly ILog _log = LogManager.GetLogger(typeof(RestSharpConnection));

		private readonly IRestClientFactory _restClientFactory;

		private readonly ICredentialStore _serviceCredentialStore;

		private readonly string _youtrackScopeId;

		private OAuth2AccessToken _resourceOwnerToken;

		#endregion

		#region Constructors and Destructors

		public RestSharpConnection(
			Uri apiBaseUrl,
			IRestClientFactory restClientFactory,
			ICredentialStore serviceCredentialStore,
			string clientId,
			string clientSecret,
			string youtrackScopeId)
		{
			if (apiBaseUrl == null)
			{
				throw new ArgumentNullException("apiBaseUrl");
			}
			if (restClientFactory == null)
			{
				throw new ArgumentNullException("restClientFactory");
			}
			if (serviceCredentialStore == null)
			{
				throw new ArgumentNullException("serviceCredentialStore");
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
			_serviceCredentialStore = serviceCredentialStore;
			_clientId = clientId;
			_clientSecret = clientSecret;
			_youtrackScopeId = youtrackScopeId;
		}

		#endregion

		#region Public Properties

		public bool IsAuthenticated
		{
			get
			{
				if (_resourceOwnerToken == null && _serviceCredentialStore.Credential == null)
				{
					return false;
				}
				if (_resourceOwnerToken != null)
				{
					return !_resourceOwnerToken.IsExpired;
				}
				return !_serviceCredentialStore.Credential.IsExpired;
			}
		}

		#endregion

		#region Public Methods and Operators

		public void Authenticate(OAuth2AccessToken accessToken)
		{
			if (accessToken == null)
			{
				throw new ArgumentNullException("accessToken`");
			}
			if (accessToken.IsExpired)
			{
				throw new ArgumentException("Access Token is Expired", "accessToken");
			}

			_resourceOwnerToken = accessToken;
		}

		public OAuth2AccessToken Authenticate(string username, string password)
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

			_resourceOwnerToken = response.Data;
			_resourceOwnerToken.DateObtained = dateObtained;

			return _resourceOwnerToken;
		}

		public ApiResponse Delete(string resource, IDictionary<string, string> routeParameters = null, IDictionary<string,string> requestParameters = null)
		{
			EnsureAuthenticated();

			var request = new RestRequest(string.Format("{0}/{1}", YouTrackRestResourceBase, resource), Method.DELETE);

			AddRouteParameters(request, routeParameters);

			AddRequestParameters(request, requestParameters);

			var response = GetClient().Execute(request);

			EnsureExpectedResponseStatus(response, HttpStatusCode.OK, HttpStatusCode.Accepted);

			return response.AsApiResponse();
		}

		public T Get<T>(string resource, IDictionary<string, string> requestParameters = null, IDictionary<string, string> routeParameters = null)
			where T : new()
		{
			EnsureAuthenticated();

			var request = new RestRequest(string.Format("{0}/{1}", YouTrackRestResourceBase, resource), Method.GET);

			AddRouteParameters(request, routeParameters);

			AddRequestParameters(request, requestParameters);

			IRestResponse<T> response = GetClient().Execute<T>(request);

			EnsureExpectedResponseStatus(response, HttpStatusCode.OK);

			return response.Data;
		}

		public IEnumerable<TInternal> Get<TWrapper, TInternal>(string resource, IDictionary<string, string> requestParameters = null)
			where TWrapper : class, IDataWrapper<TInternal>, new() where TInternal : new()
		{
			EnsureAuthenticated();

			// TInternal data = GetWrappedData<TInternal, TWrapper>(command); 

			var request = new RestRequest(string.Format("{0}/{1}", YouTrackRestResourceBase, resource), Method.GET)
				              {
					              RequestFormat = DataFormat.Json
				              };

			AddRequestParameters(request, requestParameters);

			var response = GetClient().Execute<TWrapper>(request);

			EnsureExpectedResponseStatus(response, HttpStatusCode.OK);

			return response.Data.Data ?? new TInternal[0];
		}

		public CurrentUserInfo GetCurrentAuthenticatedUser()
		{
			EnsureAuthenticated();

			var user = Get<CurrentUserInfo>(string.Format("{0}/{1}", YouTrackRestResourceBase, "user/current"));

			return user;
		}

		public IEnumerable<T> GetList<T>(
			string resource,
			IDictionary<string, string> requestParameters = null,
			IDictionary<string, string> routeParameters = null) where T : new()
		{
			EnsureAuthenticated();

			var request = new RestRequest(string.Format("{0}/{1}", YouTrackRestResourceBase, resource), Method.GET);

			AddRequestParameters(request, requestParameters);

			AddRouteParameters(request, routeParameters);

			IRestResponse<List<T>> response = GetClient().Execute<List<T>>(request);

			if (response.ResponseStatus != ResponseStatus.Completed)
			{
				throw response.ErrorException;
			}

			EnsureExpectedResponseStatus(response, HttpStatusCode.OK);

			return response.Data;
		}

		public TInternal GetWrappedData<TInternal, TWrapper>(string command) where TWrapper : class, IDataWrapper<TInternal>, new()
			// where TInternal : class
		{
			EnsureAuthenticated();

			var request = new RestRequest(string.Format("{0}/{1}", YouTrackRestResourceBase, command), Method.GET);
			var response = GetClient().Execute<TWrapper>(request);

			EnsureExpectedResponseStatus(response, HttpStatusCode.OK);

			return response.Data.Data != null ? response.Data.Data.First() : default(TInternal);
		}

		public ApiResponse Head(string resource, IDictionary<string, string> requestParameters = null)
		{
			EnsureAuthenticated();

			var request = new RestRequest(string.Format("{0}/{1}", YouTrackRestResourceBase, resource), Method.HEAD);

			AddRequestParameters(request, requestParameters);

			var response = GetClient().Execute(request);

			if (response.ResponseStatus != ResponseStatus.Completed)
			{
				throw new ConnectionException(string.Format("HEAD request to {0} failed.", request.Resource), response.ErrorException);
			}

			return response.AsApiResponse();
		}

		public void Logout()
		{
			_resourceOwnerToken = null;
		}

		public ApiResponse<T> Post<T>(
			string resource,
			object data = null,
			IDictionary<string, string> postParameters = null,
			IDictionary<string, string> requestParameters = null,
			IDictionary<string, string> routeParameters = null,
			DataSerializationFormat dataFormat = DataSerializationFormat.Json) where T : new()
		{
			EnsureAuthenticated();

			var fullPath = string.Format("{0}/{1}", YouTrackRestResourceBase, resource);

			var request = BuildPutPostRequest(fullPath, routeParameters, requestParameters, postParameters, data, dataFormat: dataFormat);

			var response = GetClient().Execute<T>(request);

			EnsureExpectedResponseStatus(response, HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.Accepted);

			return response.AsApiResponse();
		}

		public ApiResponse Post(
			string resource,
			object data = null,
			IDictionary<string, string> postParameters = null,
			IDictionary<string, string> requestParameters = null,
			IDictionary<string, string> routeParameters = null,
			DataSerializationFormat dataFormat = DataSerializationFormat.Json)
		{
			EnsureAuthenticated();

			var request = BuildPutPostRequest(
				string.Format("{0}/{1}", YouTrackRestResourceBase, resource),
				routeParameters,
				requestParameters,
				postParameters,
				data,
				dataFormat: dataFormat);

			var response = GetClient().Execute(request);

			EnsureExpectedResponseStatus(response, HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.Accepted);

			return response.AsApiResponse();
		}

		public ApiResponse PostFile(string resource, string path, string fileName = null)
		{
			EnsureAuthenticated();

			var contentType = GetFileContentType(path);
			var request = new RestRequest(string.Format("{0}/{1}", YouTrackRestResourceBase, resource), Method.POST);
			request.AddFile("file", File.ReadAllBytes(path), !string.IsNullOrEmpty(fileName) ? fileName : Path.GetFileName(path), contentType);
			request.AddHeader("Content-type", "application/json");
			request.AddHeader("Accept", "application/json");
			request.RequestFormat = DataFormat.Json;
			var response = GetClient().Execute(request);

			EnsureExpectedResponseStatus(response, HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.Accepted);

			return response.AsApiResponse();
		}

		public ApiResponse Put(
			string resource,
			object data = null,
			IDictionary<string, string> requestParameters = null,
			IDictionary<string, string> routeParameters = null,
			IDictionary<string, string> putParameters = null,
			DataSerializationFormat dataFormat = DataSerializationFormat.Json)
		{
			EnsureAuthenticated();

			var request = BuildPutPostRequest(
				string.Format("{0}/{1}", YouTrackRestResourceBase, resource),
				routeParameters,
				requestParameters,
				putParameters,
				data,
				Method.PUT,
				dataFormat);

			var response = GetClient().Execute(request);

			EnsureExpectedResponseStatus(response, HttpStatusCode.OK, HttpStatusCode.Accepted, HttpStatusCode.Created);

			return response.AsApiResponse();
		}

		public ApiResponse<T> Put<T>(
			string resource,
			object data = null,
			IDictionary<string, string> requestParameters = null,
			IDictionary<string, string> routeParameters = null,
			IDictionary<string, string> putParameters = null,
			DataSerializationFormat dataFormat = DataSerializationFormat.Json) where T : new()
		{
			EnsureAuthenticated();

			var request = BuildPutPostRequest(
				string.Format("{0}/{1}", YouTrackRestResourceBase, resource),
				routeParameters,
				requestParameters,
				putParameters,
				data,
				Method.PUT,
				dataFormat);

			if (data != null)
			{
				request.AddBody(data);
			}

			var response = GetClient().Execute<T>(request);

			EnsureExpectedResponseStatus(response, HttpStatusCode.OK, HttpStatusCode.Accepted, HttpStatusCode.Created);

			return response.AsApiResponse();
		}

		#endregion

		#region Methods

		private static void AddRequestParameters(IRestRequest request, IDictionary<string, string> requestParameters)
		{
			if (requestParameters != null)
			{
				foreach (var kvp in requestParameters)
				{
					request.AddParameter(kvp.Key, kvp.Value);
				}
			}
		}

		private static RestRequest BuildPutPostRequest(
			string resource,
			IDictionary<string, string> routeParameters,
			IDictionary<string, string> requestParameters,
			IDictionary<string, string> postParameters,
			object postBody,
			Method requestMethod = Method.POST,
			DataSerializationFormat dataFormat = DataSerializationFormat.Json)
		{
			if (postBody != null && postParameters != null && postParameters.Any())
			{
				throw new ArgumentException("Pass only non-null postBody or postParameters ... NOT BOTH", "postBody");
			}

			var requestResourceBuilder = new StringBuilder(resource);

			if (requestParameters != null && requestParameters.Any())
			{
				requestResourceBuilder.Append("?");
			}
			var firstParam = true;

			if (requestParameters != null)
			{
				foreach (var kvp in requestParameters)
				{
					requestResourceBuilder.AppendFormat("{0}{1}={{{1}}}", !firstParam ? "&" : string.Empty, kvp.Key);
					firstParam = false;
				}
			}

			var request = new RestRequest(requestResourceBuilder.ToString(), requestMethod)
				              {
					              RequestFormat = dataFormat == DataSerializationFormat.Json ? DataFormat.Json : DataFormat.Xml,
					              JsonSerializer = new NewtonsoftJsonSerializer()
				              };

			if (request.RequestFormat == DataFormat.Xml)
			{
				request.XmlSerializer.ContentType = "application/xml; charset=utf-8";
				
			}

			if (requestParameters != null)
			{
				foreach (var kvp in requestParameters)
				{
					request.AddUrlSegment(kvp.Key, kvp.Value);
				}
			}

			if (routeParameters != null)
			{
				foreach (var routeParam in routeParameters)
				{
					request.AddUrlSegment(routeParam.Key, routeParam.Value);
				}
			}

			if (postBody != null)
			{
				request.AddBody(postBody);
			}
			else if (postParameters != null)
			{
				foreach (var p in postParameters)
				{
					request.AddParameter(p.Key, p.Value);
				}
			}

			return request;
		}

		private void AddRouteParameters(IRestRequest request, IDictionary<string, string> routeParameters)
		{
			if (routeParameters != null)
			{
				foreach (var routeParam in routeParameters)
				{
					request.AddUrlSegment(routeParam.Key, routeParam.Value);
				}
			}
		}

		private void EnsureAuthenticated()
		{
			// in the absence of an explicit call to the Authenticate method, we use the Client Credentials workflow to obtain an access token for YouTrack
			// see https://www.jetbrains.com/hub/help/1.0/Client-Credentials.html

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
				var accessToken = response.Data;
				accessToken.DateObtained = dateObtained;
				_serviceCredentialStore.Credential = accessToken;
			}
		}

		private void EnsureExpectedResponseStatus(IRestResponse response, params HttpStatusCode[] expectedStatuses)
		{
			/*
			 ---------------------------
			  Error
			 ---------------------------
				An error occurred: An error has occurred.

				API request returned an unexpected response: BadRequest (Bad Request) -- Expected one of OK 

				 {"param":{"name":"workItem"},"cause":"HTTP 404 Not Found"} 
 
			---------------------------
			OK   
			---------------------------
			*/

			_log.Debug(
				string.Format(
					"Checking request status for {0} {1} \n\n {2}",
					response.Request.Method,
					response.Request.Resource,
					response.Request.Parameters.Where(p => p.Type == ParameterType.UrlSegment)
						.Aggregate(string.Empty, (str, parameter) => string.Format("{0}{1}={2}\n", str, parameter.Name, parameter.Value))));

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

				throw new HttpStatusCodeException(
					response.AsApiResponse(),
					string.Format(
						"API request returned an unexpected response: {0} ({1}) -- Expected one of {2} \n\n {3}",
						response.StatusCode,
						response.StatusDescription,
						expectedStatuses.Select(s => s.ToString()).Aggregate((s1, s2) => string.Format("{0} {1}", s1, s2)),
						response.Content));
			}
		}

		private IRestClient GetClient(bool skipAuthentication = false)
		{
			IAuthenticator authenticator = null;

			if (IsAuthenticated && !skipAuthentication)
			{
				var token = _resourceOwnerToken ?? _serviceCredentialStore.Credential;
				authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(token.AccessToken, token.TokenType);
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