using Autofac;
using Autofac.Extensions.DependencyInjection;

using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;

using Scissorhands.Helpers;
using Scissorhands.Models.Loggers;
using Scissorhands.Models.Settings;
using Scissorhands.Services;
using Scissorhands.Themes;

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
        /// <param name="appEnv"><see cref="IApplicationEnvironment"/> instance.</param>
        /// <param name="configuration"><see cref="IConfiguration"/> instance.</param>
        /// <returns>Returns the <see cref="IContainer"/> instance.</returns>
        public static IContainer Register(IServiceCollection services, IHostingEnvironment env, IApplicationEnvironment appEnv, IConfiguration configuration)
        {
            var builder = new ContainerBuilder();

            RegisterAppSettings(builder, env, appEnv, configuration);
            RegisterLoaders(builder);
            RegisterHelpers(builder);
            RegisterServices(builder);

            builder.Populate(services);

            var container = builder.Build();
            return container;
        }

        private static void RegisterAppSettings(ContainerBuilder builder, IHostingEnvironment env, IApplicationEnvironment appEnv, IConfiguration configuration)
        {
            builder.RegisterInstance(env).SingleInstance();
            builder.RegisterInstance(appEnv).SingleInstance();
            builder.RegisterInstance(configuration.Get<Logging>("Logging")).SingleInstance();
            builder.RegisterInstance(configuration.Get<WebAppSettings>("WebAppSettings")).SingleInstance();
            builder.RegisterInstance(configuration.Get<SiteMetadataSettings>("SiteMetadataSettings")).SingleInstance();
        }

        private static void RegisterLoaders(ContainerBuilder builder)
        {
            builder.RegisterType<ThemeLoader>().As<IThemeLoader>().PropertiesAutowired().InstancePerLifetimeScope();
        }

        private static void RegisterHelpers(ContainerBuilder builder)
        {
            builder.RegisterType<FileHelper>().As<IFileHelper>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<HttpRequestHelper>().As<IHttpRequestHelper>().PropertiesAutowired().InstancePerLifetimeScope();
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