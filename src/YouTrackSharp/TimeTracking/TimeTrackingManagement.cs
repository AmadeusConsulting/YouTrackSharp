using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using YouTrackSharp.Import;
using YouTrackSharp.Infrastructure;
using YouTrackSharp.Projects;

namespace YouTrackSharp.TimeTracking
{
	public class TimeTrackingManagement
	{
		private readonly IConnection _connection;

		public TimeTrackingManagement(IConnection connection)
		{
			if (connection == null)
			{
				throw new ArgumentNullException("connection");
			}
			_connection = connection;
		}

		protected TimeTrackingManagement()
		{
		}

		public virtual void CreateWorkItem(string issueId, WorkItem workItem)
		{
			if (string.IsNullOrEmpty(issueId))
			{
				throw new ArgumentNullException("issueId");
			}

			var response = _connection.Post(string.Format("issue/{0}/timetracking/workitem/", issueId), workItem);
			var locationHeader = response.Headers.Where(kvp => kvp.Key == "Location").Select(kvp => kvp.Value).SingleOrDefault();
			if (locationHeader != null)
			{
				var locationUri = new Uri(locationHeader);
				workItem.Id = locationUri.AbsolutePath.Split('/').Last();
			}
		}

		public virtual IEnumerable<WorkItem> ListWorkItems(string issueId)
		{
			if (string.IsNullOrEmpty(issueId))
			{
				throw new ArgumentNullException("issueId");
			}

			return _connection.GetList<WorkItem>(string.Format("issue/{0}/timetracking/workitem/", issueId));
		}

		public virtual void DeleteWorkItem(string issueId, string workItemId)
		{
			try
			{
				_connection.Delete(string.Format("issue/{0}/timetracking/workitem/{1}", issueId, workItemId));
			}
			catch (HttpStatusCodeException ex)
			{
				// if we get a 404 trying to delete a resource, assume it's already been deleted ... otherwise rethrow the exception
				if (ex.Response.StatusCode != HttpStatusCode.NotFound && !(ex.Response.ResponseContent != null && ex.Response.ResponseContent.Contains("HTTP 404 Not Found")))

				{
					throw;
				}
			}
		}

		public virtual void UpdateWorkItem(string issueId, WorkItem workItem)
		{
			if (string.IsNullOrEmpty(issueId))
			{
				throw new ArgumentNullException("issueId");
			}
			if (workItem == null)
			{
				throw new ArgumentNullException("workItem");
			}

			if (string.IsNullOrEmpty(workItem.Id))
			{
				throw new ArgumentException("WorkItem must have an ID in order to call update.", "workItem");
			}

			_connection.Put(string.Format("issue/{0}/timetracking/workitem/{1}", issueId, workItem.Id), workItem);
		}

		public virtual ImportResponse ImportWorkItems(string issueId, IEnumerable<WorkItem> workItems)
		{
			if (string.IsNullOrEmpty(issueId))
			{
				throw new ArgumentNullException("issueId");
			}
			if (workItems == null)
			{
				throw new ArgumentNullException("workItems");
			}

			var response = _connection.Put<ImportResponse>(string.Format("import/issue/{0}/workitems", issueId), workItems.ToList());

			return response.Data;
		}
	}
}
