using System;

using Aliencube.Scissorhands.Services;
using Aliencube.Scissorhands.Services.Helpers;
using Aliencube.Scissorhands.Services.Interfaces;

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

            RegisterCommandlineOptions(builder, options);
            RegisterRazorEngine(builder);
            RegisterServices(builder);

            _container = builder.Build();

            Execute();
        }

        /// <summary>
        /// Executes the application.
        /// </summary>
        private static void Execute()
        {
            using (var scope = _container.BeginLifetimeScope())
            {
                var service = scope.Resolve<IPublishService>();

                try
                {
                    var result = service.Process();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ooops!");
                    Console.WriteLine();
                    Console.WriteLine("    {0}", ex.Message);
                }
            }
        }

        private static void RegisterCommandlineOptions(ContainerBuilder builder, CommandOptions options)
        {
            builder.RegisterInstance(options).As<ICommandOptions>();
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

            builder.Register(c => new Markdown() { ExtraMode = true, SafeMode = false });
        }

        private static void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<PublishService>().As<IPublishService>();
        }
    }
}