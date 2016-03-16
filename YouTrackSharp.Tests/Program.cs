using System;
using System.Reflection;

using NUnit.ConsoleRunner;

namespace YouTrackSharp.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Runner.Main(new[] { Assembly.GetExecutingAssembly().Location });

            Console.ReadKey();
        }
    }
}
