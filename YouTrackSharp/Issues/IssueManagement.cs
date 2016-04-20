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
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

using YouTrackSharp.Infrastructure;
using YouTrackSharp.Projects;

using HttpException = EasyHttp.Infrastructure.HttpException;

namespace YouTrackSharp.Issues
{
	public class IssueManagement
	{
		private static readonly List<string> PredefinedIssueFields = new List<string> { "id", "jiraid", "entityid", "summary", "description", "permittedgroup" };

		readonly IConnection _connection;

		public IssueManagement(IConnection connection)
		{
			_connection = connection;
		}

		/// <summary>
		/// Retrieve an issue by id
		/// </summary>
		/// <param name="issueId">Id of the issue to retrieve</param>
		/// <returns>An instance of Issue if successful or InvalidRequestException if issues is not found</returns>
		public virtual Issue GetIssue(string issueId)
		{
			try
			{
				dynamic issue = _connection.Get<Issue>("issue/{issueId}", routeParameters: new Dictionary<string, string> { { "issueId", issueId } });

				return issue;
			}
			catch (HttpStatusCodeException ex)
			{
				if (ex.Response.StatusCode == HttpStatusCode.NotFound)
				{
					return null;
				}
				throw;
			}
			catch (HttpException exception)
			{
				throw new InvalidRequestException(
						String.Format(Language.YouTrackClient_GetIssue_Issue_not_found___0_, issueId), exception);
			}
		}

		public virtual string CreateIssue(Issue issue, string projectId, string permittedGroup = null, bool disableNotifications = false)
		{
			if (string.IsNullOrEmpty(projectId))
			{
				throw new ArgumentNullException("projectId");
			}

			if (!_connection.IsAuthenticated)
			{
				throw new InvalidRequestException(Language.YouTrackClient_CreateIssue_Not_Logged_In);
			}

			try
			{
				dynamic dynamicIssue = issue;

				if (string.IsNullOrEmpty(dynamicIssue.Summary as string))
				{
					throw new ArgumentException("Issue must have a summary.", "issue");
				}

				var requestParams = new Dictionary<string,string>
					                    {
						                    {"project", projectId},
											{"summary", dynamicIssue.Summary}
					                    };

				if (!string.IsNullOrEmpty(dynamicIssue.Description as string))
				{
					requestParams["Description"] = dynamicIssue.Description;
				}

				if (!string.IsNullOrEmpty(permittedGroup))
				{
					requestParams["permittedGroup"] = permittedGroup;
				}

				var apiResponse = _connection.Put<Issue>("issue", putParameters: requestParams);

				var createdIssueId = Regex.Match(apiResponse.Headers["Location"], @"issue/(?<issueId>[^/]+)$").Groups["issueId"].Value;

				var customFields = issue.ToExpandoObject().Where(field => !PredefinedIssueFields.Contains(field.Key.ToLower())).ToDictionary(field => field.Key, field => field.Value);

				foreach (var customField in customFields)
				{
					if (customField.Value == null)
					{
						continue;
					}

					var commandValue = customField.Value.ToString().Trim();
					if (Regex.IsMatch(commandValue, @"\s"))
					{
						commandValue = string.Format("{{{0}}}", commandValue);
					}

					ApplyCommand(createdIssueId, string.Format("{0} {1}", customField.Key, commandValue), disableNotifications: disableNotifications);
				}

				return createdIssueId;
			}
			catch (HttpException httpException)
			{
				throw new InvalidRequestException(httpException.StatusDescription, httpException);
			}
		}


		/// <summary>
		/// Retrieves a list of issues 
		/// </summary>
		/// <param name="projectIdentifier">Project Identifier</param>
		/// <param name="max">[Optional] Maximum number of issues to return. Default is int.MaxValue</param>
		/// <param name="start">[Optional] The number by which to start the issues. Default is 0. Used for paging.</param>
		/// <returns>List of Issues</returns>
		public virtual IEnumerable<Issue> GetAllIssuesForProject(string projectIdentifier, int max = int.MaxValue, int start = 0)
		{
			return _connection.GetList<Issue>(
				string.Format("issue/byproject/{0}", projectIdentifier),
				new Dictionary<string, string>
					{
						{ "max", max.ToString(CultureInfo.InvariantCulture) },
						{ "start", start.ToString(CultureInfo.InvariantCulture) }
					});
		}

		/// <summary>
		/// Retrieve comments for a particular issue
		/// </summary>
		/// <param name="issueId"></param>
		/// <returns></returns>
		public virtual IEnumerable<Comment> GetCommentsForIssue(string issueId)
		{
			return _connection.GetList<Comment>(String.Format("issue/comments/{0}", issueId));
		}

		public virtual bool CheckIfIssueExists(string issueId)
		{
			var response = _connection.Head(string.Format("issue/{0}/exists", issueId));
			return response.StatusCode == HttpStatusCode.OK;
		}

		public virtual void AttachFileToIssue(string issuedId, string path, string attachmentFileName = null)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentNullException("path");
			}
			if (!File.Exists(path))
			{
				throw new ArgumentException(string.Format("File does not exist at path {0}", path), "path");
			}

			var response = _connection.PostFile(string.Format("issue/{0}/attachment", issuedId), path, attachmentFileName);

			if (response.StatusCode != HttpStatusCode.Created)
			{
				throw new InvalidRequestException(response.StatusCode.ToString());
			}
		}

		public virtual void DeleteAttachment(string issueId, string attachmentId)
		{
			try
			{
				_connection.Delete(string.Format("issue/{0}/attachment/{1}", issueId, attachmentId));
			}
			catch (HttpStatusCodeException ex)
			{
				if (ex.Response.StatusCode != HttpStatusCode.NotFound)
				{
					throw;
				}
			}
		}

		public virtual void ApplyCommand(string issueId, string command, string comment = null, bool disableNotifications = false, string runAs = "")
		{
			if (!_connection.IsAuthenticated)
			{
				throw new InvalidRequestException(Language.YouTrackClient_CreateIssue_Not_Logged_In);
			}

			try
			{
				var commandMessage = new Dictionary<string, string> { { "command", command } };

				if (!string.IsNullOrEmpty(comment))
				{
					commandMessage["comment"] = comment;
				}
				if (disableNotifications)
				{
					commandMessage["disableNotifications"] = disableNotifications.ToString().ToLowerInvariant();
				}
				if (!string.IsNullOrWhiteSpace(runAs))
				{
					commandMessage["runAs"] = runAs;
				}

				_connection.Post(
					"issue/{issueId}/execute",
					postParameters: commandMessage,
					routeParameters: new Dictionary<string, string> { { "issueId", issueId } });
			}
			catch (HttpException httpException)
			{
				throw new InvalidRequestException(httpException.StatusDescription, httpException);
			}
		}

		public virtual void UpdateIssue(string issueId, string summary, string description)
		{
			if (!_connection.IsAuthenticated)
			{
				throw new InvalidRequestException(Language.YouTrackClient_CreateIssue_Not_Logged_In);
			}

			try
			{
				var postParameters = new Dictionary<string, string> { { "summary", summary }, { "description", description } };

				_connection.Post("issue/{issueId}", postParameters: postParameters, routeParameters: new Dictionary<string, string> { { "issueId", issueId } });
			}
			catch (HttpException httpException)
			{
				throw new InvalidRequestException(httpException.StatusDescription, httpException);
			}
		}

		public virtual IEnumerable<Issue> GetIssuesBySearch(string searchString, int max = int.MaxValue, int start = 0)
		{
			return _connection.Get<MultipleIssueWrapper, Issue>(
				"issue",
				new Dictionary<string, string>
					{
						{ "filter", searchString },
						{ "max", max.ToString(CultureInfo.InvariantCulture) },
						{ "after", start.ToString(CultureInfo.InvariantCulture) }
					});
		}

		public virtual int GetIssueCount(string searchString)
		{
			try
			{
				var count = -1;

				while (count < 0)
				{
					var countObject = _connection.Get<Count>("issue/count", requestParameters: new Dictionary<string, string> { { "filter", searchString } });

					count = countObject.Entity.Value;
					Thread.Sleep(3000);
				}

				return count;
			}
			catch (HttpException httpException)
			{
				throw new InvalidRequestException(httpException.StatusDescription, httpException);
			}
		}

		public virtual void Delete(string id)
		{
			_connection.Delete(string.Format("issue/{0}", id));
		}

		public virtual void DeleteComment(string issueId, string commentId, bool deletePermanently)
		{
			_connection.Delete(string.Format("issue/{0}/comment/{1}?permanently={2}", issueId, commentId, deletePermanently));
		}

		public Tag GetTag(string tagName)
		{
			try
			{
				return _connection.Get<Tag>("user/tag/{tagName}", routeParameters: new Dictionary<string, string> { { "tagName", tagName } });
			}
			catch (HttpStatusCodeException ex)
			{
				if (ex.Response.StatusCode == HttpStatusCode.NotFound)
				{
					return null;
				}
				throw;
			}
		}

		public void CreateTag(string name, string visibleForGroup = null, string updatableByGroup = null, bool untagOnResolve = false)
		{
			var requestParams = new Dictionary<string, string>();

			if (!string.IsNullOrEmpty(visibleForGroup))
			{
				requestParams["visibleForGroup"] = visibleForGroup;
			}

			if (!string.IsNullOrEmpty(updatableByGroup))
			{
				requestParams["updatableByGroup"] = updatableByGroup;
			}

			requestParams["untagOnResolve"] = untagOnResolve.ToString().ToLowerInvariant();

			_connection.Put("user/tag/{tagName}", null, requestParameters: requestParams, routeParameters: new Dictionary<string, string> { { "tagName", name } });
		}
	}
}