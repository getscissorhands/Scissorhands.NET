using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;

namespace Scissorhands.WebApp.Configs
{
    /// <summary>
    /// This represents the configuration entity for environment.
    /// </summary>
    public static class EnvironmentConfig
    {
        /// <summary>
        /// Registers application settings for environment.
        /// </summary>
        /// <param name="app"><see cref="IApplicationBuilder"/> instance.</param>
        /// <param name="env"><see cref="IHostingEnvironment"/> instance.</param>
        public static void Register(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
        }
    }
}