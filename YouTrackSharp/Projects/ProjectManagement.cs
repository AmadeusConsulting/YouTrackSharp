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
using System.Linq;

using YouTrackSharp.Infrastructure;

namespace YouTrackSharp.Projects
{
	public class ProjectManagement
	{
		#region Fields

		private readonly IConnection _connection;

		#endregion

		#region Constructors and Destructors

		public ProjectManagement(IConnection connection)
		{
			try
			{
				_connection = connection;
			}
			catch (ConnectionException e)
			{
				Console.WriteLine(e);
			}
		}

		#endregion

		#region Public Methods and Operators

		public virtual void AddSubsystem(string projectId, string subsystem, string description = null, string owner = null, string colorIndex = null)
		{
			var bundleName = GetProjectSubsystemBundleName(projectId);

			_connection.Put(
				"admin/customfield/ownedFieldBundle/{bundleName}/{subsystem}",
				routeParameters: new Dictionary<string, string> { { "bundleName", bundleName }, { "subsystem", subsystem } });
		}

		public virtual void AddVersion(Project project, ProjectVersion version)
		{
			throw new NotImplementedException();
		}

		public virtual void DeleteVersion(string bundleName, string versionName)
		{
			_connection.Delete(string.Format("admin/customfield/versionBundle/{0}/{1}", bundleName, versionName));
		}

		public virtual IEnumerable<ProjectIssueTypes> GetIssueTypes()
		{
			return _connection.GetList<ProjectIssueTypes>("project/types");
		}

		public virtual IEnumerable<ProjectPriority> GetPriorities()
		{
			return _connection.GetList<ProjectPriority>("project/priorities");
		}

		public virtual Project GetProject(string projectName)
		{
			return _connection.Get<Project>("admin/project/{projectName}", routeParameters: new Dictionary<string, string> { { "projectName", projectName } });
		}

		public virtual IEnumerable<Project> GetProjects()
		{
			return _connection.GetList<Project>("project/all");
		}

		public virtual IEnumerable<WorkType> GetProjectWorkTypes(object projectShortname)
		{
			return _connection.GetList<WorkType>(string.Format("admin/project/{0}/timetracking/worktype", projectShortname));
		}

		public virtual IEnumerable<ProjectResolutionType> GetResolutions()
		{
			return _connection.GetList<ProjectResolutionType>("project/resolutions");
		}

		public virtual IEnumerable<ProjectState> GetStates()
		{
			return _connection.GetList<ProjectState>("project/states");
		}

		public virtual IEnumerable<ProjectVersion> GetVersions(string versionBundleName)
		{
			var x = _connection.Get<VersionBundle>(
				"admin/customfield/versionBundle/{versionBundleName}",
				routeParameters: new Dictionary<string, string> { { "versionBundleName", versionBundleName } });
			return x.Version;
		}

		public virtual IEnumerable<ProjectVersion> GetVersions(Project project)
		{
			return GetVersions(project.VersionBundleName());
		}

		public virtual IEnumerable<OwnedField> ListSubsystems(string projectId)
		{
			var subsystemBundle = GetProjectSubsystemBundleName(projectId);

			var ownedFieldBundle = _connection.Get<OwnedFieldBundle>(
				"admin/customfield/ownedFieldBundle/{bundleName}",
				routeParameters: new Dictionary<string, string> { { "bundleName", subsystemBundle } });

			if (ownedFieldBundle == null)
			{
				return Enumerable.Empty<OwnedField>();
			}

			return ownedFieldBundle.OwnedFields ?? Enumerable.Empty<OwnedField>();
		}

		#endregion

		#region Methods

		private string GetProjectSubsystemBundleName(string projectId)
		{
			var subsystemCustomField = _connection.Get<CustomField>(
				"admin/project/{projectId}/customfield/subsystem",
				routeParameters: new Dictionary<string, string> { { "projectId", projectId } });

			var bundle = subsystemCustomField.Parameters != null
				? subsystemCustomField.Parameters.SingleOrDefault(p => p.Name.Equals("bundle", StringComparison.OrdinalIgnoreCase))
				: null;

			if (subsystemCustomField == null || string.IsNullOrEmpty(subsystemCustomField.Name) || bundle == null || string.IsNullOrEmpty(bundle.Value))
			{
				throw new InvalidOperationException(string.Format("Project {0} is missing a Custom Field for Subsystem with an OwnedField Bundle!", projectId));
			}

			return bundle.Value;
		}

		#endregion
	}
}