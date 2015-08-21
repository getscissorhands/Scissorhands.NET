using System;

using CommandLine;

namespace Scissorhands.ConsoleApp
{
    /// <summary>
    /// This represents the entity as the main entry point of the console app.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Process the console app from here.
        /// </summary>
        /// <param name="args">
        /// The list of arguments from the commandline.
        /// </param>
        internal static void Main(string[] args)
        {
            var options = new CommandOptions();
            if (!Parser.Default.ParseArguments(args, options))
            {
                Console.WriteLine("[ERROR]: failed parsing command line options.");
                return;
            }
        }
    }
}