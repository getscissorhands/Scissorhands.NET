using System;

using Aliencube.Scissorhands.WebApp;

using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;

namespace Aliencube.Scissorhands.ConsoleApp
{
    /// <summary>
    /// This represents the main entry point of the console application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Defines the main entry point of the console application.
        /// </summary>
        /// <param name="args">List of arguments.</param>
        public static void Main(string[] args)
        {
            var env = new HostingEnvironment();
            var startup = new Startup(env, args);
            var logger = new LoggerFactory();
            Action<IApplicationBuilder, IHostingEnvironment, ILoggerFactory> app = startup.Configure;
            Func<IServiceCollection, IServiceProvider> services = startup.ConfigureServices;

            var server = TestServer.Create((builder) => app(builder, env, logger), services);
            var client = server.CreateClient();
            var result = client.GetStringAsync("http://localhost/home/index").Result;

            Console.WriteLine(result);
            Console.ReadLine();
        }
    }
}