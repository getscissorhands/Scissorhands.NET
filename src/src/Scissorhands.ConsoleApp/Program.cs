using System;
using System.Threading.Tasks;

using Aliencube.Scissorhands.Services;
using Aliencube.Scissorhands.Services.Configs;
using Aliencube.Scissorhands.Services.Helpers;
using Aliencube.Scissorhands.Services.Processors;
using Aliencube.Scissorhands.Services.Wrappers;

using Autofac;

using CommandLine;

using MarkdownDeep;

using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace Aliencube.Scissorhands.ConsoleApp
{
    /// <summary>
    /// This represents the entity as the main entry point of the console app.
    /// </summary>
    internal static class Program
    {
        private static IContainer _container;

        /// <summary>
        /// Process the console app from here.
        /// </summary>
        /// <param name="args">
        /// The list of arguments from the commandline.
        /// </param>
        internal static void Main(string[] args)
        {
            var options = new CommandOptions();
            if (!Parser.Default.ParseArguments(args, options))
            {
                Console.WriteLine("[ERROR]: failed parsing command line options.");
                return;
            }

            var builder = new ContainerBuilder();

            RegisterSettings(builder);
            RegisterRazorEngine(builder);
            RegisterHelpers(builder);
            RegisterProcessors(builder);
            RegisterServices(builder);

            _container = builder.Build();

            Execute(options).Wait();
        }

        /// <summary>
        /// Executes the application.
        /// </summary>
        /// <param name="options">
        /// The <see cref="CommandOptions"/> instance.
        /// </param>
        /// <returns>
        /// Returns the <see cref="Task" />.
        /// </returns>
        private static async Task Execute(ICommandOptions options)
        {
            string postpath = null;
            if (options != null)
            {
                postpath = options.Post;
            }

            using (var scope = _container.BeginLifetimeScope())
            {
                var service = scope.Resolve<IPublishService>();

                try
                {
                    await service.PublishAsync(postpath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ooops!");
                    Console.WriteLine();
                    Console.WriteLine("    {0}", ex.Message);
                }
            }
        }

        private static void RegisterSettings(ContainerBuilder builder)
        {
            builder.Register(c => YamlSettings.Load()).As<IYamlSettings>();
        }

        private static void RegisterRazorEngine(ContainerBuilder builder)
        {
            builder.RegisterType<TemplateServiceConfiguration>()
                   .As<ITemplateServiceConfiguration>()
                   .WithProperty("BaseTemplateType", typeof(HtmlTemplate<>))
                   .WithProperty("DisableTempFileLocking", true)
                   .WithProperty("CachingProvider", new DefaultCachingProvider(t => { }));

            builder.Register(c => RazorEngineService.Create(c.Resolve<ITemplateServiceConfiguration>()))
                   .As<IRazorEngineService>();

            var md = new Markdown() { ExtraMode = true, SafeMode = false };
            builder.RegisterType<MarkdownWrapper>().As<IMarkdownWrapper>().WithParameter("md", md);
        }

        private static void RegisterHelpers(ContainerBuilder builder)
        {
            builder.RegisterType<PublishHelper>().As<IPublishHelper>();
        }

        private static void RegisterProcessors(ContainerBuilder builder)
        {
            builder.RegisterType<PostProcessor>().As<IPostProcessor>();
        }

        private static void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<PublishService>().As<IPublishService>();
        }
    }
}