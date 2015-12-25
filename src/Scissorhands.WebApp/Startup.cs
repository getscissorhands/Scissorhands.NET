using System;

using Aliencube.Scissorhands.Models;
using Aliencube.Scissorhands.WebApp.Configs;

using Autofac;

using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Aliencube.Scissorhands.WebApp
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
        /// Gets or sets the configuration.
        /// </summary>
        public IConfigurationRoot Configuration { get; set; }

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
            logger.AddConsole(this.Configuration.GetSection("Logging"));
            logger.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            var settings = this.Configuration.Get<WebAppSettings>("WebAppSettings");

            if (settings.Server == ServerType.Iis)
            {
                app.UseIISPlatformHandler();
            }

            app.UseStaticFiles();

            app.UseMvc(
                routes =>
                    {
                        routes.MapRoute(name: "default", template: "{controller=Home}/{action=Index}/{id?}");
                    });
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

            var container = DependencyConfig.Register(services, this.Configuration);
            return container.Resolve<IServiceProvider>();
        }
    }
}