#region License

// Distributed under the BSD License
//   
// YouTrackSharp Copyright (c) 2010-2012, Hadi Hariri and Contributors
// All rights reserved.
//   
//  Redistribution and use in source and binary forms, with or without
//  modification, are permitted provided that the following conditions are met:
//      * Redistributions of source code must retain the above copyright
//         notice, this list of conditions and the following disclaimer.
//      * Redistributions in binary form must reproduce the above copyright
//         notice, this list of conditions and the following disclaimer in the
//         documentation and/or other materials provided with the distribution.
//      * Neither the name of Hadi Hariri nor the
//         names of its contributors may be used to endorse or promote products
//         derived from this software without specific prior written permission.
//   
//   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
//   "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
//   TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
//   PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL 
//   <COPYRIGHTHOLDER> BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
//   SPECIAL,EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
//   LIMITED  TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
//   DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND  ON ANY
//   THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//   (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
//   THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//   

#endregion

using System;
using System.Collections.Generic;

using YouTrackSharp.Admin;
using YouTrackSharp.Projects;

namespace YouTrackSharp.Infrastructure
{
	public interface IConnection
	{
		#region Public Properties

		/// <summary>
		///     Gets a value indicating whether this instance is authenticated.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is authenticated; otherwise, <c>false</c>.
		/// </value>
		bool IsAuthenticated { get; }

		#endregion

		#region Public Methods and Operators

		/// <summary>
		///     Authenticates with the specified Resource Owner username and password.
		/// </summary>
		/// <param name="username">The username.</param>
		/// <param name="password">The password.</param>
		OAuth2AccessToken Authenticate(string username, string password);

		/// <summary>
		///     Authenticates using the specified OAuth2 access token.
		/// </summary>
		/// <param name="accessToken">The access token.</param>
		/// <exception cref="ArgumentException">Thrown if the access token is <c>null</c> or expired</exception>
		void Authenticate(OAuth2AccessToken accessToken);

		/// <summary>
		///     Performs a Delete command on the specified resource.
		/// </summary>
		/// <param name="resource">The resource.</param>
		/// <returns></returns>
		ApiResponse Delete(string resource);

		/// <summary>
		///     Gets the specified resource.
		/// </summary>
		/// <typeparam name="T">The type to deserialize from the response</typeparam>
		/// <param name="resource">The resource.</param>
		/// <param name="requestParameters"></param>
		/// <param name="routeParameters"></param>
		/// <returns></returns>
		T Get<T>(string resource, IDictionary<string, string> requestParameters = null, IDictionary<string, string> routeParameters = null) where T : new();

		/// <summary>
		///     Gets the specified resource.
		/// </summary>
		/// <typeparam name="TWrapper">The type of the wrapper.</typeparam>
		/// <typeparam name="TInternal">The type of the internal.</typeparam>
		/// <param name="resource">The resource.</param>
		/// <param name="requestParameters"></param>
		/// <returns></returns>
		IEnumerable<TInternal> Get<TWrapper, TInternal>(string resource, IDictionary<string, string> requestParameters = null)
			where TWrapper : class, IDataWrapper<TInternal>, new() where TInternal : new();

		/// <summary>
		///     Gets the current authenticated user.
		/// </summary>
		/// <returns></returns>
		User GetCurrentAuthenticatedUser();

		/// <summary>
		///     Gets a list of entities from the given resourcce.
		/// </summary>
		/// <typeparam name="TEntity">The type of entity</typeparam>
		/// <param name="resource">The resource.</param>
		/// <param name="requestParameters"></param>
		/// <param name="routeParameters"></param>
		/// <returns></returns>
		IEnumerable<TEntity> GetList<TEntity>(string resource, 
			IDictionary<string, string> requestParameters = null,
			IDictionary<string,string> routeParameters = null) where TEntity : new();

		ApiResponse Head(string resource, IDictionary<string, string> requestParameters = null);

		/// <summary>
		///     Destroy the current authenticated session.
		/// </summary>
		void Logout();

		/// <summary>
		///     Posts the specified resource.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="resource">The resource.</param>
		/// <param name="data">The data.</param>
		/// <param name="postParameters">The post parameters.</param>
		/// <param name="requestParameters">The request parameters.</param>
		/// <param name="routeParameters"></param>
		/// <returns></returns>
		ApiResponse<T> Post<T>(
			string resource,
			object data = null,
			IDictionary<string, string> postParameters = null,
			IDictionary<string, string> requestParameters = null,
			IDictionary<string, string> routeParameters = null) where T : new();

		/// <summary>
		///     Posts the specified resource.
		/// </summary>
		/// <param name="resource">The resource.</param>
		/// <param name="data">The data.</param>
		/// <param name="postParameters">The post parameters.</param>
		/// <param name="requestParameters">The request parameters.</param>
		/// <param name="routeParameters"></param>
		/// <returns></returns>
		ApiResponse Post(
			string resource,
			object data = null,
			IDictionary<string, string> postParameters = null,
			IDictionary<string, string> requestParameters = null,
			IDictionary<string, string> routeParameters = null);

		/// <summary>
		///     Posts a file to the specified resource.
		/// </summary>
		/// <param name="resource">The resource.</param>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		ApiResponse PostFile(string resource, string path);

		/// <summary>
		///     Puts the specified resource.
		/// </summary>
		/// <param name="resource">The resource.</param>
		/// <param name="data">The data.</param>
		/// <param name="requestParameters">The request parameters.</param>
		/// <param name="routeParameters"></param>
		/// <param name="putParameters"></param>
		/// <returns></returns>
		/// <remarks>
		///     Request parameters must be embedded in the resource to be replaced as route parameters.
		///     So, if you need to make a request to foo/bar?baz=1 .. you will need to format your
		///     resource like <code>"foo/bar?baz={bazValue}"</code> and pass bazValue in the request parameters.
		/// </remarks>
		ApiResponse Put(
			string resource,
			object data = null,
			IDictionary<string, string> requestParameters = null,
			IDictionary<string, string> routeParameters = null,
			IDictionary<string, string> putParameters = null);

		/// <summary>
		///     Puts the specified resource.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="resource">The resource.</param>
		/// <param name="data">The data.</param>
		/// <param name="requestParameters">The request parameters.</param>
		/// <param name="routeParameters"></param>
		/// <param name="putParameters"></param>
		/// <returns></returns>
		/// ///
		/// <remarks>
		///     Request parameters must be embedded in the resource to be replaced as route parameters.
		///     So, if you need to make a request to foo/bar?baz=1 .. you will need to format your
		///     resource like <code>"foo/bar?baz={bazValue}"</code> and pass bazValue in the request parameters.
		/// </remarks>
		ApiResponse<T> Put<T>(
			string resource,
			object data = null,
			IDictionary<string, string> requestParameters = null,
			IDictionary<string, string> routeParameters = null,
			IDictionary<string, string> putParameters = null) where T : new();

		#endregion
	}
}