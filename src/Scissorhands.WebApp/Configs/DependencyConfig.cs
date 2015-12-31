using Autofac;
using Autofac.Extensions.DependencyInjection;

using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Scissorhands.Helpers;
using Scissorhands.Models.Loggers;
using Scissorhands.Models.Settings;
using Scissorhands.Services;

namespace Scissorhands.WebApp.Configs
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
        /// <param name="env"><see cref="IHostingEnvironment"/> instance.</param>
        /// <param name="configuration"><see cref="IConfiguration"/> instance.</param>
        /// <returns>Returns the <see cref="IContainer"/> instance.</returns>
        public static IContainer Register(IServiceCollection services, IHostingEnvironment env, IConfiguration configuration)
        {
            var builder = new ContainerBuilder();

            RegisterAppSettings(builder, env, configuration);
            RegisterHelpers(builder);
            RegisterServices(builder);

            builder.Populate(services);

            var container = builder.Build();
            return container;
        }

        private static void RegisterAppSettings(ContainerBuilder builder, IHostingEnvironment env, IConfiguration configuration)
        {
            builder.RegisterInstance(env).SingleInstance();
            builder.RegisterInstance(configuration.Get<Logging>("Logging")).SingleInstance();
            builder.RegisterInstance(configuration.Get<WebAppSettings>("WebAppSettings")).SingleInstance();
        }

        private static void RegisterHelpers(ContainerBuilder builder)
        {
            builder.RegisterType<FileHelper>().As<IFileHelper>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<MarkdownHelper>().As<IMarkdownHelper>().PropertiesAutowired().InstancePerLifetimeScope();
        }

        private static void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<BuildService>().As<IBuildService>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<MarkdownService>().As<IMarkdownService>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<PublishService>().As<IPublishService>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<ThemeService>().As<IThemeService>().PropertiesAutowired().InstancePerLifetimeScope();
        }
    }
}