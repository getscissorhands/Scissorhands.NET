using System;
using System.Threading.Tasks;

namespace Scissorhands
{
    /// <summary>
    /// This represents the main entiry of the console application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// This is called when the console application is running.
        /// </summary>
        /// <param name="args">List of arguments from the command-line.</param>
        public static void Main(string[] args)
        {
            var options = CommandOptionsBuilder.Default.BuildAsync(args).Result;

        }
    }
}
