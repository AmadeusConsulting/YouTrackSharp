using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Newtonsoft.Json;

namespace YouTrackSharp.Issues
{
	public class ListIssue : Issue
	{
		/*
         "priority": "3",
      "type": "Bug",
      "state": "Submitted",
      "subsystem": "No Subsystem",
      "affectsVersion": null,
      "id": "NAT-1",
      "fixedVersion": null,
      "projectShortName": "NAT",
      "assigneeName": null,
      "reporterName": "Meagan_Combs",
      "updaterName": "Meagan_Combs",
      "fixedInBuild": "Next build",
      "commentsCount": 0,
      "numberInProject": 1,
      "summary": "test",
      "description": null,
      "created": 1443729466358,
      "updated": 1444855950067,
      "historyUpdated": 1444855950067,
      "resolved": null,
      "jiraId": null,
      "votes": 0,
      "permittedGroup": null,
         */

		public string Priority { get; set; }
		public string Type { get; set; }
		public string State { get; set; }
		public string Subsystem { get; set; }
		public string AffectsVersion { get; set; }
		public string FixedVersion { get; set; }
		public string ProjectShortName { get; set; }
		public string AssigneeName { get; set; }
		public string ReporterName { get; set; }
		public string UpdaterName { get; set; }
		public string FixedInBuild { get; set; }
		public int CommentsCount { get; set; }
		public int NumberInProject { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public decimal? DueDate { get; set; }
        public decimal? Estimate { get; set; }
		//   [JsonDateFormat(JsonDateFormat.TimestampMillis)]
		public DateTime Created { get; set; }
		public DateTime Updated { get; set; }
		public DateTime HistoryUpdated { get; set; }
		public string Resolved { get; set; }
		public int Votes { get; set; }
		public string PermittedGroup { get; set; }

        private string _caseCompleteId;
        //[JsonProperty(PropertyName = "case complete id")]
        public string CaseCompleteId
        {
            get
            {
                return string.IsNullOrEmpty(_caseCompleteId) ? (string) GetValue("Case Complete ID") : _caseCompleteId;
            }
            set { _caseCompleteId = value; }
        }
	  
	    private List<Attachment> _attachments;

		[JsonProperty(PropertyName = "attachments")]
		public List<Attachment> Attachments
		{
			get { return _attachments ?? (_attachments = new List<Attachment>()); }
			set { _attachments = value; }
		}

		private List<Link> _links;

		[JsonProperty(PropertyName = "attachments")]
		public List<Link> Links
		{
			get { return _links ?? (_links = new List<Link>()); }
			set { _links = value; }
		}

        public void SetCustomFields()
        {
            var fieldList = ToExpandoObject();

            //Priority = (string) GetValue(fieldList, "Priority");
                //Estimate = (string)GetValue(fieldList, "Id"),
                //DueDate = (string)GetValue(fieldList, "Id"),
            //CaseCompleteId = (string) GetValue(fieldList, "Case Complete ID");
            //CommentsCount = (string)GetValue(fieldList, "Id"),
            //Created = (string)GetValue(fieldList, "Id"),
            //FixedInBuild = (string)GetValue(fieldList, "Id"),
            //FixedVersion = (string)GetValue(fieldList, "Id"),
            //HistoryUpdated = (string)GetValue(fieldList, "Id"),
            //NumberInProject = (string)GetValue(fieldList, "Id"),
            //Resolved = (string)GetValue(fieldList, "Id"),
            //UpdaterName = (string)GetValue(fieldList, "Id"),
            //Votes = (string)GetValue(fieldList, "Id"),
            //Updated = (string)GetValue(fieldList, "Id"),
            //ReporterName = (string)GetValue(fieldList, "Id"),
            //PermittedGroup = (string)GetValue(fieldList, "Id"),
            //State = (string)GetValue(fieldList, "Id"),
        }

        private object GetValue(string key)
        {
            var valueArray = (Fields.Where(f => f.name == key).Select(f => f.value).FirstOrDefault() as ArrayList);
            if (valueArray == null || valueArray.Count == 0) return null;
            return valueArray[0];
        }
	}
}