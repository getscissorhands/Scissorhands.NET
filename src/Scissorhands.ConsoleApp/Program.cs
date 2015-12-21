using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;

using Scissorhands.WebApp;

namespace Aliencube.Scissorhands.ConsoleApp
{
    //public class Startup
    //{
    //    public Startup(IHostingEnvironment env)
    //    {
    //    }

    //    public void ConfigureServices(IServiceCollection services)
    //    {
    //        services.AddMvc();
    //    }

    //    public void Configure(IApplicationBuilder app)
    //    {
    //        // Configure the HTTP request pipeline.
    //        app.UseStaticFiles();

    //        // Add MVC to the request pipeline.
    //        app.UseMvc();
    //    }
    //}

    public class Program
    {
        private IHostingEnvironment _env;

        private Startup _startup;

        private IConfiguration _config;
        private Action<IApplicationBuilder> _app;
        private IServiceProvider _services;

        public static void Main(string[] args)
        {
            var environment = CallContextServiceLocator.Locator.ServiceProvider.GetRequiredService<IApplicationEnvironment>();

            var env = new HostingEnvironment();
            var startup = new Startup(env, args);
            var logger = new LoggerFactory();
            Action<IApplicationBuilder, IHostingEnvironment, ILoggerFactory> app = startup.Configure;
            Action<IServiceCollection> services = startup.ConfigureServices;

            var server = TestServer.Create((builder) => app(builder, env, logger), services);
            var client = server.CreateClient();
            var result = client.GetStringAsync("http://localhost/home/index").Result;

            Console.WriteLine(result);
            ////var env = new HostingEnvironment();
            ////var startup = new Startup(env);

            ////var builder = TestServer.CreateBuilder(startup.Configuration);
            ////builder.
            
            ////this._config = this._startup.Configuration;
            ////this._services = CallContextServiceLocator.Locator.ServiceProvider;
            ////_app = this._startup.Configure();

            //var x = TestMe();
            //x.Wait();
            //Console.WriteLine(x.Result);

            Console.ReadLine();
        }

        public static async Task<string> TestMe()
        {
            var server = TestServer.Create();
            var client = server.CreateClient();
            return await client.GetStringAsync("http://localhost/home/index");
        }
    }
}
