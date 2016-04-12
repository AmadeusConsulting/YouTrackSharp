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
using System.Collections.ObjectModel;
using System.Globalization;
using System.Security;

using YouTrackSharp.Infrastructure;

namespace YouTrackSharp.Admin
{
	public class UserManagement
	{
		readonly IConnection _connection;

		public UserManagement(IConnection connection)
		{
			_connection = connection;
		}

		public IEnumerable<User> ListUsers(string query = null, string groupId = null, string role = null, string projectId = null, string permission = null, bool onlineOnly = false, int start = 0)
		{
			var requestParameters = new Dictionary<string,string>
				                        {
					                        {"start", start.ToString(CultureInfo.InvariantCulture)},
											{"onlineOnly", onlineOnly.ToString().ToLowerInvariant()}
				                        };

			if (!string.IsNullOrEmpty(query))
			{
				requestParameters["q"] = query;
			}
			if (!string.IsNullOrEmpty(groupId))
			{
				requestParameters["group"] = groupId;
			}
			if (!string.IsNullOrEmpty(role))
			{
				requestParameters["role"] = role;
			}
			if (!string.IsNullOrEmpty(projectId))
			{
				requestParameters["project"] = projectId;
			}
			if (!string.IsNullOrEmpty(permission))
			{
				requestParameters["permission"] = permission;
			}

			IEnumerable<User> userItems = _connection.GetList<User>("admin/user", requestParameters);

			return userItems;
		}

		public UserDetail GetUser(string login)
		{
			return _connection.Get<UserDetail>(
				"admin/user/{login}",
				routeParameters: new Dictionary<string, string>
					                 {
						                 { "login", login }
					                 });
		}
	}
}