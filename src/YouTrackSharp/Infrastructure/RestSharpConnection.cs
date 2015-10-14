using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Security.Authentication;
using RestSharp.Authenticators;
using RestSharp.Validation;
using YouTrackSharp.Admin;
using YouTrackSharp.Projects;
using RestSharp;


namespace YouTrackSharp.Infrastructure
{
    public class RestSharpConnection : IConnection
    {
    
        private readonly IRestClient _client;
        private readonly Uri _baseUri;
        private string p;
        private System.Uri uri;

        public RestSharpConnection(Uri apiBaseUrl, IRestClient client)
        {
            if (apiBaseUrl == null)
            {
                throw new ArgumentNullException("apiBaseUrl");
            }
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }
            _baseUri = apiBaseUrl;
            _client = client;
        }

        //public RestSharpConnection(string p)
        //{
        //    // TODO: Complete member initialization
        //    this.p = p;
        //}

        //public RestSharpConnection(string p)
        //{
        //    // TODO: Complete member initialization
        //    this.p = p;
        //}

        //public RestSharpConnection(System.Uri uri)
        //{
        //    // TODO: Complete member initialization
        //    this.uri = uri;
        //}

        public bool IsAuthenticated
        {
            get {
                return true; //_client.CookieContainer.GetCookies(_baseUri).Cast<Cookie>().Where(c => (c.Name == "" || c.Name == "") && !c.Expired).Count() == 2;
            }
        }

        public HttpStatusCode HttpStatusCode
        {
            get;
            private set;
        }

        public IEnumerable<T> GetList<T>(string command) where T : new()
        {
             var request = new RestRequest(command, Method.GET);
       
           IRestResponse<List<T>> response = _client.Execute<List<T>>(request);
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

        public T Get<T>(string command) where T : new()
        {
           var request = new RestRequest(command, Method.GET);

           IRestResponse<T> response = _client.Execute<T>(request);
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

        public TInternal GetWrappedData<TInternal,TWrapper>(string command) 
            where TWrapper : class, IDataWrapper<TInternal>, new() 
           // where TInternal : class
        {
            var request = new RestRequest(command, Method.GET);
            var response = _client.Execute<TWrapper>(request);

            return response.Data.Data != null ? response.Data.Data.First() : default(TInternal);
        }

        public System.Collections.Generic.IEnumerable<TInternal> Get<TWrapper, TInternal>(string command) where TWrapper : class, 
            IDataWrapper<TInternal>, new() where TInternal : new()
        {
       
          // TInternal data = GetWrappedData<TInternal, TWrapper>(command); 

            var request = new RestRequest(command, Method.GET);
            request.RequestFormat = DataFormat.Json;
            var response = _client.Execute<TWrapper>(request);

            return response.Data.Data != null ? response.Data.Data : new TInternal[0];
     
        }

        public dynamic Post(string command, object data, string accept)
        {
            var request = new RestRequest(command, Method.POST);
            request.AddHeader("Accept", accept);
            request.AddBody(data);
            

            var response = _client.Execute<dynamic>(request);
            return response.Data;
        }

        public void Authenticate(string username, string password)
        {
            _client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest("user/login?username={username}&password={password}", Method.POST);
            request.AddUrlSegment("username", username);
            request.AddUrlSegment("password", password);
          
            var response = _client.Execute(request);

            HttpStatusCode = response.StatusCode;

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new AuthenticationException(Language.YouTrackClient_Login_Authentication_Failed);
            }

        }

        public void Logout()
        {
            throw new System.NotImplementedException();
        }

        public User GetCurrentAuthenticatedUser()
        {
            var user = Get<User>("user/current");

            if (user != null)
            {
                return user;
            }

            return null;
        }

        public void PostFile(string command, string path)
        {
            var contentType = GetFileContentType(path);
            var request = new RestRequest(command, Method.POST);
            request.AddFile("file", File.ReadAllBytes(path), contentType);
           
        }

        public void Head(string command)
        {
            throw new System.NotImplementedException();
        }

        public void Post(string command, object data)
        {

            var request = new RestRequest(command, Method.POST);
            request.AddBody(data);
            var response = _client.Execute(request);

            HttpStatusCode = response.StatusCode;

            if (response.ResponseStatus != ResponseStatus.Completed)
            {
                throw response.ErrorException;
            }
            
        }

        public void Put(string command, object data)
        {
            var request = new RestRequest(command, Method.PUT);
            var response = _client.Execute(request);

            HttpStatusCode = response.StatusCode;

            if (response.ResponseStatus != ResponseStatus.Completed)
            {
                throw response.ErrorException;
            }
            
        }

        public void Delete(string command)
        {
            var request = new RestRequest(command, Method.DELETE);
            var response = _client.Execute(request);

            HttpStatusCode = response.StatusCode;

            if(response.StatusCode != HttpStatusCode.OK ){
                throw new InvalidRequestException(string.Format("Request Failed! \n{0}", response.Content));
            }
        }

        string GetFileContentType(string filename)
        {
            var mime = "application/octetstream";
            var extension = Path.GetExtension(filename);
            if (extension != null)
            {
                var ext = extension.ToLower();
                var rk = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
                if (rk != null && rk.GetValue("Content Type") != null)
                    mime = rk.GetValue("Content Type").ToString();
            }
            return mime;
        }
     }
}
