using System;
using System.Xml.Schema;

using NDesk.Options;

using Newtonsoft.Json;

using YouTrackSharp.Infrastructure;
using YouTrackSharp.Projects;

namespace YouTrackSharp.TestHarness
{
	class Program
	{

		private static string Username { get; set; }
		private static string Password { get; set; }
		private static string YouTrackUrl { get; set; }
		private static string ClientId { get; set; }
		private static string ScopeId { get; set; }
		private static string ClientSecret { get; set; }

		private static OptionSet sOptions = new OptionSet
			                                    {
				                                    { "u|username=", u => Username = u },
				                                    { "p|password=", p => Password = p },
				                                    { "y|youtrack-url=", f => YouTrackUrl = f },
				                                    { "s|scope-id=", s => ScopeId = s },
				                                    { "c|client-id=", c => ClientId = c },
				                                    { "k|client-secret=", s => ClientSecret = s }
			                                    };

		private static void Main(string[] args)
		{
			sOptions.Parse(args);

			Validate();

			if (string.IsNullOrEmpty(Password))
			{
			    Console.Write("Enter YouTrack Password for {0}: ", Username);
				Password = ReadPassword();
			}

			var connection = new RestSharpConnection(
				new Uri(YouTrackUrl),
				new DefaultRestClientFactory(),
				new ThreadSafeCredentialStore(),
				ClientId,
				ClientSecret,
				ScopeId);

			connection.Authenticate(Username, Password);

			var projectMgt = new ProjectManagement(connection);

			var bundleName = projectMgt.CreateProjectSpecificOwnedFieldBundleForCustomField("ACTSC", "Subsystem");

			Console.WriteLine(bundleName);

			var bundle = projectMgt.GetOwnedFieldBundle(bundleName);

			Console.WriteLine(JsonConvert.SerializeObject(bundle));

			projectMgt.DeleteOwnedFieldBundle(bundleName);

			Console.ReadKey(true);
		}

		private static void Validate()
		{
			bool isValid = true;
			if (string.IsNullOrEmpty(Username))
			{
				Console.WriteLine("Missing Username");
				isValid = false;
			}
			if (string.IsNullOrEmpty(YouTrackUrl))
			{
				Console.WriteLine("Missing YouTrackUrl");
				isValid = false;
			}
			if (string.IsNullOrEmpty(ClientId))
			{
				Console.WriteLine("Missing ClientId");
				isValid = false;
			}
			if (string.IsNullOrEmpty(ScopeId))
			{
				Console.WriteLine("Missing Scope Id");
				isValid = false;
			}
			if (string.IsNullOrEmpty(ClientSecret))
			{
				Console.WriteLine("Missing Client Secret");
				isValid = false;
			}

			if (!isValid)
			{
				System.Environment.Exit(1);
			}
		}

		private static string ReadPassword()
		{
			string password = "";
			ConsoleKeyInfo info = Console.ReadKey(true);
			while (info.Key != ConsoleKey.Enter)
			{
				if (info.Key != ConsoleKey.Backspace)
				{
					Console.Write("*");
					password += info.KeyChar;
				}
				else if (info.Key == ConsoleKey.Backspace)
				{
					if (!string.IsNullOrEmpty(password))
					{
						// remove one character from the list of password characters
						password = password.Substring(0, password.Length - 1);
						// get the location of the cursor
						int pos = Console.CursorLeft;
						// move the cursor to the left by one character
						Console.SetCursorPosition(pos - 1, Console.CursorTop);
						// replace it with space
						Console.Write(" ");
						// move the cursor to the left by one character again
						Console.SetCursorPosition(pos - 1, Console.CursorTop);
					}
				}
				info = Console.ReadKey(true);
			}
			// add a new line because user pressed enter at the end of their password
			Console.WriteLine();
			return password;
		}
	}
}
