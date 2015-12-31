using System;

using Autofac;

using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Scissorhands.WebApp.Configs;

namespace Scissorhands.WebApp
{
    /// <summary>
    /// This represents the main entry point of the web application.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env"><see cref="IHostingEnvironment"/> instance.</param>
        public Startup(IHostingEnvironment env)
            : this(env, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env"><see cref="IHostingEnvironment"/> instance.</param>
        /// <param name="args">List of arguments from the command line.</param>
        public Startup(IHostingEnvironment env, string[] args)
        {
            this.HostingEnvironment = env;

            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();

            if (args != null && args.Length > 0)
            {
                builder.AddCommandLine(args);
            }

            this.Configuration = builder.Build();
        }

        /// <summary>
        /// Gets the hosting environment.
        /// </summary>
        public IHostingEnvironment HostingEnvironment { get; }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public IConfigurationRoot Configuration { get; }

        /// <summary>
        /// Defines the main entry point of the console application.
        /// </summary>
        /// <param name="args">List of arguments.</param>
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);

        /// <summary>
        /// Configures modules.
        /// </summary>
        /// <param name="app"><see cref="IApplicationBuilder"/> instance.</param>
        /// <param name="env"><see cref="IHostingEnvironment"/> instance.</param>
        /// <param name="logger"><see cref="ILoggerFactory"/> instance.</param>
        /// <remarks>This method gets called by the runtime. Use this method to configure the HTTP request pipeline.</remarks>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory logger)
        {
            LoggerConfig.Register(logger, this.Configuration);
            EnvironmentConfig.Register(app, env);
            WebServerConfig.Register(app, this.Configuration);
        }

        /// <summary>
        /// Configures services including dependencies.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> instance.</param>
        /// <remarks>This method gets called by the runtime. Use this method to add services to the container.</remarks>
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            // Add dependencies.
            var container = DependencyConfig.Register(services, this.HostingEnvironment, this.Configuration);
            return container.Resolve<IServiceProvider>();
        }
    }
}