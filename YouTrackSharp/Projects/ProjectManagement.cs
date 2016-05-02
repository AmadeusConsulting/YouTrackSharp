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
using System.Globalization;
using System.Linq;

using YouTrackSharp.Infrastructure;

namespace YouTrackSharp.Projects
{
	public class ProjectManagement : ManagementBase
	{

		#region Constructors and Destructors

		public ProjectManagement(IConnection connection) : base(connection)
		{
		}

		#endregion

		#region Public Methods and Operators

		public virtual void AddCustomFieldToProject(string projectId, string customFieldName, string emptyText = null, string defaultValue = null)
		{
			if (string.IsNullOrEmpty(projectId))
			{
				throw new ArgumentNullException("projectId");
			}

			var parameters = new Dictionary<string, string>();

			if (emptyText != null)
			{
				parameters["emptyText"] = emptyText;
			}
			if (defaultValue != null)
			{
				parameters["defaultValue"] = defaultValue;
			}

			Connection.Put(
				"admin/project/{projectId}/customfield/{customFieldName}",
				routeParameters: new Dictionary<string, string>
					                 {
						                 { "projectId", projectId },
						                 { "customFieldName", customFieldName }
					                 },
				putParameters: parameters);
		}

		public virtual void AddSubsystem(string projectId, string subsystem, string description = null, string owner = null, string colorIndex = null)
		{
			var bundleName = GetProjectSubsystemBundleName(projectId);

			Connection.Put(
				"admin/customfield/ownedFieldBundle/{bundleName}/{subsystem}",
				routeParameters: new Dictionary<string, string>
					                 {
						                 { "bundleName", bundleName },
						                 { "subsystem", subsystem }
					                 });
		}

		public virtual void AddVersion(Project project, ProjectVersion version)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     Creates the owned field bundle.
		/// </summary>
		/// <param name="bundleName">Name of the bundle. Must be unique; auto-generated if not supplied.</param>
		/// <param name="copyValuesFromBundle">The copy values from bundle.</param>
		/// <param name="values">The values.</param>
		/// <returns>The new bundle name.</returns>
		public virtual string CreateOwnedFieldBundle(string bundleName = null, string copyValuesFromBundle = null, params OwnedField[] values)
		{
			var bundleValues = new List<OwnedField>();

			if (string.IsNullOrEmpty(bundleName))
			{
				bundleName = Guid.NewGuid().ToString("D");
			}

			if (!string.IsNullOrEmpty(copyValuesFromBundle))
			{
				var existingBundle = GetOwnedFieldBundle(copyValuesFromBundle);
				bundleValues.AddRange(existingBundle.OwnedFields);
			}

			if (values.Any())
			{
				bundleValues.AddRange(values);
			}

			var newBundle = new OwnedFieldBundle
				                {
					                Name = bundleName,
					                OwnedFields = bundleValues
				                };

			Connection.Put("admin/customfield/ownedFieldBundle", newBundle, dataFormat: DataSerializationFormat.Xml);

			return bundleName;
		}

		public virtual void DeleteOwnedFieldBundle(string bundleName)
		{
			Connection.Delete(
				"admin/customfield/ownedFieldBundle/{bundleName}",
				routeParameters: new Dictionary<string, string>
					                 {
						                 { "bundleName", bundleName }
					                 });
		}

		public virtual void CreateProject(
			string projectId,
			string projectName,
			string projectLeadLogin,
			int startingIssueNumber = 1,
			string description = null)
		{
			var putParams = new Dictionary<string, string>
				                {
					                { "projectName", projectName },
					                { "startingNumber", startingIssueNumber.ToString(CultureInfo.InvariantCulture) },
					                { "projectLeadLogin", projectLeadLogin }
				                };

			if (!string.IsNullOrEmpty(description))
			{
				putParams["description"] = description;
			}

			Connection.Put(
				"admin/project/{projectId}",
				routeParameters: new Dictionary<string, string>
					                 {
						                 { "projectId", projectId }
					                 },
				putParameters: putParams);
		}

		/// <summary>
		///     Creates the project specific owned field bundle for custom field.
		/// </summary>
		/// <param name="projectId">The project identifier.</param>
		/// <param name="customFieldName">Name of the field.</param>
		/// <param name="newBundleName">
		///     New name of the bundle.  Must be unique across all Owned Field bundles. Will be
		///     auto-generated if not supplied
		/// </param>
		/// <param name="copyValuesFromBundle">The copy values from bundle.</param>
		/// <param name="values">The values.</param>
		/// <returns>The new bundle name</returns>
		public virtual string CreateProjectSpecificOwnedFieldBundleForCustomField(
			string projectId,
			string customFieldName,
			string newBundleName = null,
			string copyValuesFromBundle = null,
			params OwnedField[] values)
		{
			var createdBundleName = CreateOwnedFieldBundle(newBundleName, copyValuesFromBundle, values);

			Connection.Post(
				"admin/project/{projectId}/customfield/{customFieldName}",
				requestParameters: new Dictionary<string, string>
					                   {
						                   { "bundle", createdBundleName }
					                   }, routeParameters: new Dictionary<string, string>
						                                       {
							                                       { "projectId", projectId },
							                                       { "customFieldName", customFieldName }
						                                       });

			return createdBundleName;
		}

		public virtual void DeleteVersion(string bundleName, string versionName)
		{
			Connection.Delete(string.Format("admin/customfield/versionBundle/{0}/{1}", bundleName, versionName));
		}

		public virtual IEnumerable<ProjectIssueTypes> GetIssueTypes()
		{
			return Connection.GetList<ProjectIssueTypes>("project/types");
		}

		public OwnedFieldBundle GetOwnedFieldBundle(string bundleName)
		{
			return Connection.Get<OwnedFieldBundle>(
				"admin/customfield/ownedFieldBundle/{bundleName}",
				routeParameters: new Dictionary<string, string>
					                 {
						                 { "bundleName", bundleName }
					                 });
		}

		public virtual IEnumerable<ProjectPriority> GetPriorities()
		{
			return Connection.GetList<ProjectPriority>("project/priorities");
		}

		public virtual Project GetProject(string projectName)
		{
			return Connection.Get<Project>(
				"admin/project/{projectName}",
				routeParameters: new Dictionary<string, string>
					                 {
						                 { "projectName", projectName }
					                 });
		}

		public virtual IEnumerable<Project> GetProjects()
		{
			return Connection.GetList<Project>("project/all");
		}

		public virtual IEnumerable<WorkType> GetProjectWorkTypes(object projectShortname)
		{
			return Connection.GetList<WorkType>(string.Format("admin/project/{0}/timetracking/worktype", projectShortname));
		}

		public virtual IEnumerable<ProjectResolutionType> GetResolutions()
		{
			return Connection.GetList<ProjectResolutionType>("project/resolutions");
		}

		public virtual IEnumerable<ProjectState> GetStates()
		{
			return Connection.GetList<ProjectState>("project/states");
		}

		public virtual IEnumerable<ProjectVersion> GetVersions(string versionBundleName)
		{
			var x = Connection.Get<VersionBundle>(
				"admin/customfield/versionBundle/{versionBundleName}",
				routeParameters: new Dictionary<string, string>
					                 {
						                 { "versionBundleName", versionBundleName }
					                 });
			return x.Version;
		}

		public virtual IEnumerable<ProjectVersion> GetVersions(Project project)
		{
			return GetVersions(project.VersionBundleName());
		}

		public virtual IEnumerable<OwnedField> ListSubsystems(string projectId)
		{
			var subsystemBundle = GetProjectSubsystemBundleName(projectId);

			var ownedFieldBundle = Connection.Get<OwnedFieldBundle>(
				"admin/customfield/ownedFieldBundle/{bundleName}",
				routeParameters: new Dictionary<string, string>
					                 {
						                 { "bundleName", subsystemBundle }
					                 });

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
			var subsystemCustomField = Connection.Get<CustomField>(
				"admin/project/{projectId}/customfield/subsystem",
				routeParameters: new Dictionary<string, string>
					                 {
						                 { "projectId", projectId }
					                 });

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