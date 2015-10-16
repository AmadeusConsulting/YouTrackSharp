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
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

namespace YouTrackSharp.Issues
{
    public abstract class YouTrackExpando : DynamicObject
    {

        private List<Comment> _comments;

        [JsonProperty(PropertyName = "comment")]
        public List<Comment> Comments
        {
            get { return _comments ?? (_comments = new List<Comment>()); }
            set { _comments = value; }
        }
        private List<Field> _fields;
        
        [JsonProperty(PropertyName = "field")]
        public List<Field> Fields
        {
            get { return _fields ?? (_fields = new List<Field>()); }
            set { _fields = value; }
        }

        public ExpandoObject ToExpandoObject()
        {
            IDictionary<string, object> expando = new ExpandoObject();


            foreach (var field in Fields)
            {
                if (String.Compare(field.name, "ProjectShortName", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    expando.Add("project", field.value);
                }
                else if (String.Compare(field.name, "permittedGroup", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    expando.Add("permittedGroup", field.value);
                }
                else
                {
                    expando.Add(field.name.ToLower(), field.value);
                }
            }
            return (ExpandoObject)expando;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (Fields.Any(f => f.name.Equals(binder.Name, StringComparison.OrdinalIgnoreCase)))
            {
                result = Fields.Single(f => f.name.Equals(binder.Name, StringComparison.OrdinalIgnoreCase)).value;
                return true;
            }
            return base.TryGetMember(binder, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (Fields.Any(f => f.name.Equals(binder.Name, StringComparison.OrdinalIgnoreCase)))
            {
                // update val
                Fields.Single(f => f.name.Equals(binder.Name, StringComparison.OrdinalIgnoreCase)).value = value;
            }
            else
            {
                Fields.Add(new Field {name = binder.Name, value = value});
            }
            return true;
        }
    }


    public class Issue : YouTrackExpando
    {
        public string Id { get; set; }

        public string EntityId { get; set; }

        public string JiraId { get; set; }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (binder.Name != null && binder.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
            {
                Id = value != null ? value.ToString() : null;
                return true;
            }
            
            return base.TrySetMember(binder, value);
        }
    }

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
        public string FixedInBiuld { get; set; }
        public int CommentsCount { get; set; }
        public int NumberInProject { get; set; }
        public string Summary { get; set; }
      //  [JsonProperty(ItemConverterType = typeof(DateTime))]
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public DateTime HistoryUpdated { get; set; }
        public string Resolved { get; set; }
        public int Votes { get; set; }
        public string PermittedGroup { get; set; }

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

    }
}