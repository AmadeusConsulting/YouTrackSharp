using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NSubstitute;
using NSubstitute.Core;
using NUnit.Framework.Constraints;
using YouTrackSharp.Infrastructure;
using RestSharp;
using YouTrackSharp.Issues;
using YouTrackSharp.Projects;

namespace Nexus.Admin.Test
{
    [TestFixture]
    public class YouTrackRestApiTest
    {
        private RestSharpConnection _restSharpConnection;
        private IRestClient _restClient;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
        }

        [SetUp]
        public void SetUp()
        {
            _restClient = Substitute.For<IRestClient>();
            _restSharpConnection = new RestSharpConnection(new Uri("http://yt.wolfgang.com/"), new DefaultRestClientFactory(), new ThreadSafeCredentialStore(), "", "", "");
        }

        [TearDown]
        public void TearDown()
        {
            _restClient = null;
            _restSharpConnection = null;
        }

        
        [Test]
		[Ignore]
        public void Test_Put_Request_With_Issue()
        {
            _restSharpConnection.Authenticate("mecombs@amadeusconsulting.com", "mozart");
            dynamic issue = new Issue();
            issue.Summary = "YT Test Issue";
            issue.Description = "YT test description";
            issue.ProjectShortName = "NAT";
            var comments = new List<Comment>();
            Comment comment = new Comment();
            comment.Author = "Meagan";
            comment.Text = "comment added from apitest";
            issue.Comments = comments;
            issue.Subsystem = "Documentation";
            issue.Type = "task";
            issue.Assignee = "Meagan Combs";
            issue.Priority = "Critical";
            issue.WorkHours = 8;
            issue.CustomField = "customfield";
            issue.Created = "";

            var fieldList = issue.ToExpandoObject();

            IssueManagement issueManagement = new IssueManagement(_restSharpConnection);
          //  var issueId = issueManagement.CreateIssue(issue);
            issueManagement.ApplyCommand("NAT-41", "Assignee Meagan_Combs", "");
            var issues = issueManagement.GetAllIssuesForProject("NAT");
        }


        [Test]
		[Ignore]
        public void Test_Post_File_To_Issue()
        {
            Uri uri = new Uri("http://yt.wolfgang.com/youtrack/");
            RestClient rc = new RestClient { CookieContainer = new CookieContainer(), BaseUrl = uri };
            rc.AddHandler("application/json", new NewtonsoftJsonDeserializer());
            
            _restSharpConnection.Authenticate("mecombs@amadeusconsulting.com", "mozart");
            IssueManagement issueManagement = new IssueManagement(_restSharpConnection);
            issueManagement.AttachFileToIssue("NAT-50", @"C:\Users\mecombs\Downloads\Grey-Wolf.jpg");
        }
        
    }
}
