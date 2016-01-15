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
            _restSharpConnection = new RestSharpConnection(new Uri("http://yt.wolfgang.com/youtrack/"), _restClient);
        }

        [TearDown]
        public void TearDown()
        {
            _restClient = null;
            _restSharpConnection = null;
        }

        [Test]
        public void Test_Detail_Issue_With_Custom_Properties_Deserialize()
        {
            var responseContent = ApiResponse.IssueDetail;

            //Arrange
            var restSharpDeserializer = new NewtonsoftJsonDeserializer();
            
            var mockResponse = Substitute.For<IRestResponse>();

            mockResponse.Content.Returns(responseContent);
            mockResponse.Headers.Returns(new List<Parameter> { new Parameter {  Name = "Content-Type", Value = "application/json" } });

            //Act
            dynamic issue = restSharpDeserializer.Deserialize<Issue>(mockResponse);

            //Assert
            Assert.AreEqual("NAT-2", issue.Id);
            Assert.AreEqual("Admin Login", issue.Summary);
         
        }

        [Test]
        public void Test_List_Issue_With_Custom_Properties_Deserialize()
        {
            var responseContent = ApiResponse.IssueList;

            var restSharpDeserializer = new NewtonsoftJsonDeserializer();

            var mockResponse = Substitute.For<IRestResponse>();

            mockResponse.Content.Returns(responseContent);
            mockResponse.Headers.Returns(new List<Parameter> { new Parameter { Name = "Content-Type", Value = "application/json" } });

           List<ListIssue> issues = restSharpDeserializer.Deserialize<MultipleIssueWrapper>(mockResponse).Data;

            Assert.AreEqual("NAT-1",issues.First().Id);
        }

        [Test]
        public void Test_Project_Deserialize()
        {
            var responseContent = ApiResponse.Project;

            var restSharpDeserializer = new NewtonsoftJsonDeserializer();

            var mockResponse = Substitute.For<IRestResponse>();

            mockResponse.Content.Returns(responseContent);
            mockResponse.Headers.Returns(new List<Parameter> { new Parameter { Name = "Content-Type", Value = "application/json" } });

            Project project = restSharpDeserializer.Deserialize<Project>(mockResponse);

            Assert.AreEqual("NexusAdminTest", project.Name);
        }

        [Test]
        public void Test_Project_List_Subsystems_Deserialize()
        {
            var responseContent = ApiResponse.ProjectSubsystems;

            var restSharpDeserializer = new NewtonsoftJsonDeserializer();

            var mockResponse = Substitute.For<IRestResponse>();

            mockResponse.Content.Returns(responseContent);
            mockResponse.Headers.Returns(new List<Parameter> { new Parameter { Name = "Content-Type", Value = "application/json" } });

            List<Subsystem> subsystems = restSharpDeserializer.Deserialize<List<Subsystem>>(mockResponse);

            Assert.AreEqual("Admin", subsystems.First().Name);
        }

        [Test]
        public void Test_Detail_Issue_With_Comments_Deserialize()
        {
            var responseContent = ApiResponse.DetailIssueWithComments;

            //Arrange
            var restSharpDeserializer = new NewtonsoftJsonDeserializer();

            var mockResponse = Substitute.For<IRestResponse>();

            mockResponse.Content.Returns(responseContent);
            mockResponse.Headers.Returns(new List<Parameter> { new Parameter { Name = "Content-Type", Value = "application/json" } });

            //Act
            dynamic issue = restSharpDeserializer.Deserialize<Issue>(mockResponse);
            issue.Created = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds((double)1444849398783);
            //Assert
            Assert.AreEqual("NAT-7", issue.Id);  
            Assert.AreEqual(274, issue.Created.DayOfYear);
            Assert.AreEqual(274, issue.Updated.DayOfYear);
        }

        
        [Test]
        public void Test_Put_Request_With_Issue()
        {
            Uri uri = new Uri("http://yt.wolfgang.com/youtrack/");
            RestClient rc = new RestClient { CookieContainer = new CookieContainer(), BaseUrl = uri };
            rc.AddHandler("application/json", new NewtonsoftJsonDeserializer());
            RestSharpConnection _ytConnection = new RestSharpConnection(uri, rc);
            _ytConnection.Authenticate("mecombs@amadeusconsulting.com", "mozart");
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

            //_ytConnection.Post( "rest/issue", fieldList, "application/json");

            IssueManagement issueManagement = new IssueManagement(_ytConnection);
          //  var issueId = issueManagement.CreateIssue(issue);
            issueManagement.ApplyCommand("NAT-41", "Assignee Meagan_Combs", "");
            var issues = issueManagement.GetAllIssuesForProject("NAT");
        }


        [Test]
        public void Test_Post_File_To_Issue()
        {
            Uri uri = new Uri("http://yt.wolfgang.com/youtrack/");
            RestClient rc = new RestClient { CookieContainer = new CookieContainer(), BaseUrl = uri };
            rc.AddHandler("application/json", new NewtonsoftJsonDeserializer());
            RestSharpConnection _ytConnection = new RestSharpConnection(uri, rc);
            _ytConnection.Authenticate("mecombs@amadeusconsulting.com", "mozart");
            IssueManagement issueManagement = new IssueManagement(_ytConnection);
            issueManagement.AttachFileToIssue("NAT-50", @"C:\Users\mecombs\Downloads\Grey-Wolf.jpg");
        }
        
    }
}
