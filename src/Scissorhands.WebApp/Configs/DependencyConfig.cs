using Aliencube.Scissorhands.Models;
using Aliencube.Scissorhands.Services;
using Aliencube.Scissorhands.Services.Helpers;

using Autofac;
using Autofac.Extensions.DependencyInjection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aliencube.Scissorhands.WebApp.Configs
{
    /// <summary>
    /// This represents the configuration entity for dependency injection.
    /// </summary>
    public static class DependencyConfig
    {
        /// <summary>
        /// Registers dependencies.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> instance.</param>
        /// <param name="configuration"><see cref="IConfiguration"/> instance.</param>
        /// <returns>Returns the <see cref="IContainer"/> instance.</returns>
        public static IContainer Register(IServiceCollection services, IConfiguration configuration)
        {
            var builder = new ContainerBuilder();

            RegisterAppSettings(builder, configuration);
            RegisterHelpers(builder);
            RegisterServices(builder);

            builder.Populate(services);

            var container = builder.Build();
            return container;
        }

        private static void RegisterAppSettings(ContainerBuilder builder, IConfiguration configuration)
        {
            builder.RegisterInstance(configuration.Get<Logging>("Logging")).SingleInstance();
            builder.RegisterInstance(configuration.Get<WebAppSettings>("WebAppSettings")).SingleInstance();
        }

        private static void RegisterHelpers(ContainerBuilder builder)
        {
            builder.RegisterType<FileHelper>().As<IFileHelper>().PropertiesAutowired().InstancePerLifetimeScope();
        }

        private static void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<MarkdownService>().As<IMarkdownService>().PropertiesAutowired().InstancePerLifetimeScope();
        }
    }
}