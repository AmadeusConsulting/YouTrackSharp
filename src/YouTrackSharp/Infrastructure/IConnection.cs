﻿#region License

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

using System.Collections.Generic;

using YouTrackSharp.Admin;
using YouTrackSharp.Projects;

namespace YouTrackSharp.Infrastructure
{
	public interface IConnection
	{
		#region Public Properties

		bool IsAuthenticated { get; }

		#endregion

		#region Public Methods and Operators

		void Authenticate(string username, string password);

		ApiResponse Delete(string command);

		T Get<T>(string command) where T : new();

		IEnumerable<TInternal> Get<TWrapper, TInternal>(string command) where TWrapper : class, IDataWrapper<TInternal>, new() where TInternal : new();

		User GetCurrentAuthenticatedUser();

		IEnumerable<T> GetList<T>(string command) where T : new();

		ApiResponse Head(string command);

		void Logout();

		ApiResponse<T> Post<T>(
			string command,
			object data,
			IDictionary<string, string> postParameters = null,
			params KeyValuePair<string, string>[] requestParameters) where T : new();

		ApiResponse Post(
			string command,
			object data = null,
			IDictionary<string, string> postParameters = null,
			params KeyValuePair<string, string>[] requestParameters);

		ApiResponse PostFile(string command, string path);

		ApiResponse Put(string command, object data, params KeyValuePair<string, string>[] requestParameters);

		ApiResponse<T> Put<T>(string resource, object data, params KeyValuePair<string, string>[] requestParameters) where T : new();

		#endregion
	}
}