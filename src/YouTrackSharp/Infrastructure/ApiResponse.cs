using System.Collections.Generic;
using System.Net;

namespace YouTrackSharp.Infrastructure
{
	public class ApiResponse<T> : ApiResponse
		where T : new()
	{
		public ApiResponse(T data, string responseContent, HttpStatusCode statusCode, IDictionary<string, string> headers=null) : base(responseContent, statusCode, headers)
		{
			Data = data;
		}

		public T Data { get; private set; }
	}

	public class ApiResponse
	{
		public ApiResponse(string responseContent, HttpStatusCode statusCode, IDictionary<string, string> headers = null)
		{
			ResponseContent = responseContent;
			StatusCode = statusCode;
			Headers = headers ?? new Dictionary<string, string>();
		}

		public HttpStatusCode StatusCode { get; protected set; }
		public IDictionary<string, string> Headers { get; private set; }
		public string ResponseContent { get; private set; }
	}
}