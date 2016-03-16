using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RestSharp;

namespace YouTrackSharp.Infrastructure
{
	public static class RestResponseExtensions
	{
		public static ApiResponse AsApiResponse(this IRestResponse response)
		{
			var headersDictionary = new Dictionary<string,string>();

			var headerKVPs = response.Headers.Select(p => new KeyValuePair<string, string>(p.Name, p.Value.ToString()));

			foreach (var kvp in headerKVPs)
			{
				headersDictionary[kvp.Key] = kvp.Value;
			}

			return new ApiResponse(response.Content, response.StatusCode, headersDictionary);
		}

		public static ApiResponse<T> AsApiResponse<T>(this IRestResponse<T> response) where T : new()
		{
			var headersDictionary = new Dictionary<string,string>();

			var headerKVPs = response.Headers.Select(p => new KeyValuePair<string, string>(p.Name, p.Value.ToString()));

			foreach (var kvp in headerKVPs)
			{
				headersDictionary[kvp.Key] = kvp.Value;
			}

			return new ApiResponse<T>(response.Data, response.Content, response.StatusCode, headersDictionary);
		}
	}
}
