using Aliencube.Scissorhands.Models.Settings;

using Microsoft.AspNet.Builder;
using Microsoft.Extensions.Configuration;

namespace Aliencube.Scissorhands.WebApp.Configs
{
    /// <summary>
    /// This represents the configuration entity for web server.
    /// </summary>
    public static class WebServerConfig
    {
        /// <summary>
        /// Registers application settings for web server.
        /// </summary>
        /// <param name="app"><see cref="IApplicationBuilder"/> instance.</param>
        /// <param name="config"><see cref="IConfiguration"/> instance.</param>
        public static void Register(IApplicationBuilder app, IConfiguration config)
        {
            var settings = config.Get<WebAppSettings>("WebAppSettings");

            if (settings.Server == ServerType.Iis)
            {
                app.UseIISPlatformHandler();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvc();
        }
    }
}