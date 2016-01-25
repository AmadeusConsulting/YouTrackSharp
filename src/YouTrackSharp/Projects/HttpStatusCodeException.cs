using System;
using System.Runtime.Serialization;

using YouTrackSharp.Infrastructure;

namespace YouTrackSharp.Projects
{
	[Serializable]
	public class HttpStatusCodeException : Exception
	{
		public ApiResponse Response { get; set; }

		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		public HttpStatusCodeException(ApiResponse response, string message)
			: this(response, message, null)
		{
		}

		public HttpStatusCodeException(ApiResponse response, string message = null, Exception inner = null)
			: base(message, inner)
		{
			Response = response;
		}

		protected HttpStatusCodeException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}