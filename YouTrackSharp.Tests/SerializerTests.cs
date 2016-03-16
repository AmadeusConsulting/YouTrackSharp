using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Newtonsoft.Json;

using NSubstitute;

using NUnit.Framework;

using RestSharp;

using YouTrackSharp.Extensions;
using YouTrackSharp.Infrastructure;
using YouTrackSharp.Issues;
using YouTrackSharp.Projects;

using ApiResponse = YouTrackSharp.Tests.ApiResponse;

namespace YouTrackSharp.Tests
{
	[TestFixture]
	public class SerializerTests
	{
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
			
		}

		[TearDown]
		public void TearDown()
		{
			
		}

		[Test]
		public void Test_Detail_Issue_With_Custom_Properties_Deserialize()
		{
			var responseContent = ApiResponse.IssueDetail;

			//Arrange
			var restSharpDeserializer = new NewtonsoftJsonDeserializer();

			var mockResponse = Substitute.For<IRestResponse>();

			mockResponse.Content.Returns(responseContent);
			mockResponse.Headers.Returns(new List<Parameter> { new Parameter { Name = "Content-Type", Value = "application/json" } });

			//Act
			dynamic issue = restSharpDeserializer.Deserialize<Issue>(mockResponse);

			//Assert
			Assert.AreEqual("YTM-1", issue.Id);
			Assert.AreEqual("Nexus People Sync from Active Directory", issue.Summary);
			Assert.AreEqual("YTM", issue.projectShortName);
			Assert.IsInstanceOf<IList>(issue.Subsystem);
			Assert.AreEqual(issue.Subsystem[0], "SSIS");
		}

		[Test]
		public void Test_List_Issue_With_Custom_Properties_Deserialize()
		{
			var responseContent = ApiResponse.IssueList;

			var restSharpDeserializer = new NewtonsoftJsonDeserializer();

			var mockResponse = Substitute.For<IRestResponse>();

			mockResponse.Content.Returns(responseContent);
			mockResponse.Headers.Returns(new List<Parameter> { new Parameter { Name = "Content-Type", Value = "application/json" } });

			var issues = restSharpDeserializer.Deserialize<MultipleIssueWrapper>(mockResponse).Data;

			dynamic firstIssue = issues.First();
			Assert.AreEqual("YTM-1", firstIssue.Id);
			Assert.AreEqual("YTM", firstIssue.projectShortName);
			Assert.IsInstanceOf<IList>(firstIssue.Subsystem);
			Assert.AreEqual(firstIssue.Subsystem[0], "SSIS");
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

			//Assert
			Assert.AreEqual("NAT-7", issue.Id);
			Assert.AreEqual(274, issue.Created.DayOfYear);
			Assert.AreEqual(10, issue.Updated.Month);
			Assert.AreEqual(14, issue.Updated.Day);
		}

		[Test]
		public void Test_DateTimeOffset_To_Timestamp_And_Back()
		{
			var jsonConverter = new TimestampSecondsJsonConverter();

			var date = DateTimeOffset.Now.NoTicks();
			var writer = Substitute.For<JsonWriter>();
			var serializer = Substitute.For<JsonSerializer>();
			var reader = Substitute.For<JsonReader>();
			string serialized = null;
			writer.When(jw => jw.WriteValue(Arg.Any<long>())).Do(ci => serialized = ci.Arg<long>().ToString(CultureInfo.InvariantCulture));

			jsonConverter.WriteJson(writer, date, serializer);

			reader.Value.Returns(serialized);

			var deserialized = (DateTimeOffset)jsonConverter.ReadJson(reader, typeof(DateTimeOffset), DateTimeOffset.MinValue, serializer);

			Assert.IsTrue(deserialized == date);
		}
	}
}
