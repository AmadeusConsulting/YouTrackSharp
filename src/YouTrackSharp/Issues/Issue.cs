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
using System.Text.RegularExpressions;

using Newtonsoft.Json;

using YouTrackSharp.Extensions;
using YouTrackSharp.Infrastructure;

namespace YouTrackSharp.Issues
{
	/// <summary>
	///     YouTrack Issue
	/// </summary>
	/// <example>
	///     Example JSON Issue:
	///     <c><![CDATA[
	///  {
	///   "id": "NAT-195",
	///   "entityId": "97-358",
	///   "jiraId": null,
	///   "field": [
	/// 	{
	/// 	  "name": "projectShortName",
	/// 	  "value": "NAT"
	/// 	},
	/// 	{
	/// 	  "name": "numberInProject",
	/// 	  "value": "195"
	/// 	},
	/// 	{
	/// 	  "name": "summary",
	/// 	  "value": "Report a Suggestion"
	/// 	},
	/// 	{
	/// 	  "name": "description",
	/// 	  "value": "Create a suggestion on a mobile device"
	/// 	},
	/// 	{
	/// 	  "name": "created",
	/// 	  "value": "1445874133097"
	/// 	},
	/// 	{
	/// 	  "name": "updated",
	/// 	  "value": "1452799367159"
	/// 	},
	/// 	{
	/// 	  "name": "updaterName",
	/// 	  "value": "jgawarecki"
	/// 	},
	/// 	{
	/// 	  "name": "updaterFullName",
	/// 	  "value": "Joel Gawarecki"
	/// 	},
	/// 	{
	/// 	  "name": "reporterName",
	/// 	  "value": "Meagan_Combs"
	/// 	},
	/// 	{
	/// 	  "name": "reporterFullName",
	/// 	  "value": "Meagan Combs"
	/// 	},
	/// 	{
	/// 	  "name": "commentsCount",
	/// 	  "value": "2"
	/// 	},
	/// 	{
	/// 	  "name": "votes",
	/// 	  "value": "0"
	/// 	},
	/// 	{
	/// 	  "name": "links",
	/// 	  "value": [
	/// 		{
	/// 		  "value": "NEXUS-1",
	/// 		  "type": "Subtask",
	/// 		  "role": "parent for"
	/// 		},
	/// 		{
	/// 		  "value": "NAT-214",
	/// 		  "type": "Subtask",
	/// 		  "role": "parent for"
	/// 		}
	/// 	  ]
	/// 	},
	/// 	{
	/// 	  "name": "Type",
	/// 	  "value": [
	/// 		"Use Case"
	/// 	  ],
	/// 	  "valueId": [
	/// 		"Use Case"
	/// 	  ],
	/// 	  "color": {
	/// 		"bg": "#7b35db",
	/// 		"fg": "white"
	/// 	  }
	/// 	},
	/// 	{
	/// 	  "name": "Priority",
	/// 	  "value": [
	/// 		"Critical"
	/// 	  ],
	/// 	  "valueId": [
	/// 		"Critical"
	/// 	  ],
	/// 	  "color": {
	/// 		"bg": "#ffe3e3",
	/// 		"fg": "#cc0000"
	/// 	  }
	/// 	},
	/// 	{
	/// 	  "name": "State",
	/// 	  "value": [
	/// 		"Reopened"
	/// 	  ],
	/// 	  "valueId": [
	/// 		"Reopened"
	/// 	  ],
	/// 	  "color": null
	/// 	},
	/// 	{
	/// 	  "name": "Subsystem",
	/// 	  "value": [
	/// 		"Alerts"
	/// 	  ],
	/// 	  "valueId": [
	/// 		"Alerts"
	/// 	  ],
	/// 	  "color": null
	/// 	},
	/// 	{
	/// 	  "name": "Due Date",
	/// 	  "value": [
	/// 		"1452168000000"
	/// 	  ],
	/// 	  "valueId": [
	/// 		"1452168000000"
	/// 	  ],
	/// 	  "color": null
	/// 	},
	/// 	{
	/// 	  "name": "Estimate",
	/// 	  "value": [
	/// 		"180"
	/// 	  ],
	/// 	  "valueId": [
	/// 		"3h"
	/// 	  ],
	/// 	  "color": null
	/// 	},
	/// 	{
	/// 	  "name": "Work Hours",
	/// 	  "value": [
	/// 		"720"
	/// 	  ],
	/// 	  "valueId": [
	/// 		"1d4h"
	/// 	  ],
	/// 	  "color": null
	/// 	},
	/// 	{
	/// 	  "name": "Fix versions",
	/// 	  "value": [
	/// 		"Sprint 2"
	/// 	  ],
	/// 	  "valueId": [
	/// 		"Sprint 2"
	/// 	  ],
	/// 	  "color": null
	/// 	},
	/// 	{
	/// 	  "name": "Affected versions",
	/// 	  "value": [
	/// 		"Sprint 1"
	/// 	  ],
	/// 	  "valueId": [
	/// 		"Sprint 1"
	/// 	  ],
	/// 	  "color": null
	/// 	},
	/// 	{
	/// 	  "name": "Non-Billable",
	/// 	  "value": [
	/// 		"Yes"
	/// 	  ],
	/// 	  "valueId": [
	/// 		"Yes"
	/// 	  ],
	/// 	  "color": null
	/// 	},
	/// 	{
	/// 	  "name": "ATP Area",
	/// 	  "value": [
	/// 		"Administration"
	/// 	  ],
	/// 	  "valueId": [
	/// 		"Administration"
	/// 	  ],
	/// 	  "color": null
	/// 	},
	/// 	{
	/// 	  "name": "Deployment Category",
	/// 	  "value": [
	/// 		"Test (Amadeus and Client)"
	/// 	  ],
	/// 	  "valueId": [
	/// 		"Test (Amadeus and Client)"
	/// 	  ],
	/// 	  "color": {
	/// 		"bg": "#339933",
	/// 		"fg": "white"
	/// 	  }
	/// 	},
	/// 	{
	/// 	  "name": "attachments",
	/// 	  "value": [
	/// 		{
	/// 		  "value": "tmp59F4.png",
	/// 		  "id": "113-122",
	/// 		  "url": "https:///yt.wolfgang.com/youtrack/_persistent/tmp59F4.png?file=113-122&v=0&c=true"
	/// 		}
	/// 	  ]
	/// 	}
	///   ],
	///   "comment": [
	/// 	{
	/// 	  "id": "107-329",
	/// 	  "author": "Meagan_Combs",
	/// 	  "authorFullName": "Meagan Combs",
	/// 	  "issueId": "NAT-195",
	/// 	  "parentId": null,
	/// 	  "deleted": false,
	/// 	  "jiraId": null,
	/// 	  "text": "Create a suggestion on a mobile device\n\n\n\nPreconditions\n----------\n\n\nPostConditions\n---------------\nSubmitted suggestion is shown in the My Safety list\n\nSuccess Scenario\n------------------\n1.  Login (UC-14-1)\n2.  Tap Make Us Better\n3.  User will be presented with one question per screen in the same order, with the same verbiage and answers/answer types as on the web\n\t3.1.  User will automatically be navigated upon selecting an answer to a question where it can be determined that the user is done answering\n\t3.2.  In cases where it is not known if the user has completed the question (multi-select questions, etc.), the user will swipe right to left to advance forward\n4.  Optional: Record Audio (UC-14-7)\n5.  Optional: Add Images (UC-14-8)\n6.  Optional: Delete Image (UC-14-9)\n7.  After completing all questions (or advancing without answering), user will be presented with a screen to review the entered data\n\t7.1.  Screen will scroll vertically to accommodate all information entered for the suggestion\n\t7.2.  User may tap on the edit button for any section if modifying the data in that section is desired.  Doing so will navigate the user to the first question screen for the selected section\n\t\t7.2.1.  User will continue through all subsequent questions from that point until back on the review screen\n\t7.3.  User taps Submit\n8.  User is navigated back to the My Safety screen\n\nExtensions\n---------------------\n*.a  For all questions, special rules/workflows defined within the web use cases apply to the mobile workflow as well",
	/// 	  "shownForIssueAuthor": false,
	/// 	  "created": 1445874133715,
	/// 	  "updated": null,
	/// 	  "permittedGroup": null,
	/// 	  "replies": []
	/// 	},
	/// 	{
	/// 	  "id": "107-330",
	/// 	  "author": "Meagan_Combs",
	/// 	  "authorFullName": "Meagan Combs",
	/// 	  "issueId": "NAT-195",
	/// 	  "parentId": null,
	/// 	  "deleted": false,
	/// 	  "jiraId": null,
	/// 	  "text": "User: A user within the system.  The general parent object for the different user types.",
	/// 	  "shownForIssueAuthor": false,
	/// 	  "created": 1445874134355,
	/// 	  "updated": null,
	/// 	  "permittedGroup": null,
	/// 	  "replies": []
	/// 	}
	///   ],
	///   "tag": []
	/// }
	///  ]]></c>
	/// </example>
	public class Issue : YouTrackExpando
	{
		#region Static Fields

		private static readonly Regex MillisecondDateTimestampRegex = new Regex(@"^\d{12,}$", RegexOptions.Compiled);

		#endregion

		#region Fields

		#endregion

		#region Public Properties

		[JsonDeserializeOnly]
		public string EntityId { get; set; }

		public string Id { get; set; }

		[JsonDeserializeOnly]
		public string JiraId { get; set; }

		#endregion

		#region Public Methods and Operators

		public override ExpandoObject ToExpandoObject()
		{
			dynamic expando = base.ToExpandoObject();
			expando.Id = Id;
			expando.JiraId = JiraId;
			expando.EntityId = EntityId;
			return expando;
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			var found = base.TryGetMember(binder, out result);
			if (found && IsProbablyADate(result))
			{
				if (result is string)
				{
					result = long.Parse((string)result);
				}
				
				result = ((long)result).DateTimeOffsetFromTimestamp();
			}
			return found;
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			if (value is DateTimeOffset)
			{
				value = ((DateTimeOffset)value).ToTimestampMillis();
			}
			else if (value is DateTime)
			{
				value = new DateTimeOffset(((DateTime)value).ToUniversalTime(), TimeSpan.Zero).ToTimestampMillis();
			}

			if (binder.Name != null && binder.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
			{
				Id = value != null ? value.ToString() : null;
				return true;
			}

			return base.TrySetMember(binder, value);
		}

		#endregion

		#region Methods

		private bool IsProbablyADate(object value)
		{
			if (value is long || value is int)
			{
				return (long)value >= 100000000000;
			}

			var strValue = value as string;

			if (string.IsNullOrEmpty(strValue))
			{
				return false;
			}
			return MillisecondDateTimestampRegex.IsMatch(strValue);
		}

		#endregion
	}
}