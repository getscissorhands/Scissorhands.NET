using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using Scissorhands.Core;
using Scissorhands.Core.Resolvers;

namespace Scissorhands.RouteGenerator
{
    /// <summary>
    /// This represents the console app entity.
    /// </summary>
    public class Program
    {
        private static string contentsRootDirectory;

        /// <summary>
        /// Invokes the console app.
        /// </summary>
        /// <param name="args">List of arguments.</param>
        public static async Task Main(string[] args)
        {
            var config = CreateConfigurationBuilder(args).Build();
            var pd = config.GetValue<string>("projectDirectory").Replace('/', Path.DirectorySeparatorChar);

            var urls = await GetAllUrlPathsAsync(pd);

            var template = await GetTemplateAsync(urls);
            await SetTemplateAsync(pd, template);
        }

        private static IConfigurationBuilder CreateConfigurationBuilder(string[] args)
        {
            var mappings = new Dictionary<string, string>()
            {
                { "-pd", "projectDirectory" },
                { "--project-directory", "projectDirectory" },
            };
            var builder = new ConfigurationBuilder()
                             .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                             .AddCommandLine(args, mappings);

            return builder;
        }

        private static async Task<List<string>> GetAllUrlPathsAsync(string projectDirectory)
        {
            contentsRootDirectory = Path.Combine(projectDirectory.TrimEnd(Path.DirectorySeparatorChar), Constants.Contents);

            var resolver = new ContentsResolver(contentsRootDirectory);
            var items = await resolver.ResolveAsync();

            var urls = resolver.ResolveAllUrlPaths(items).Skip(1).ToList();

            return urls;
        }

        private static async Task<string> GetTemplateAsync(List<string> urls)
        {
            var asminfo = new FileInfo(Assembly.GetExecutingAssembly().Location);

            var template = default(string);
            using (var stream = new FileStream(Path.Combine(asminfo.Directory.FullName.TrimEnd(Path.DirectorySeparatorChar), "Routing.cs.template"), FileMode.Open))
            using (var reader = new StreamReader(stream))
            {
                template = await reader.ReadToEndAsync();
                template = template.Replace("{{ ROUTE }}", string.Join("\r\n    ", urls.Select(p => $"[Route(\"{p}\")]")));
            }

            return template;
        }

        private static async Task SetTemplateAsync(string pd, string template)
        {
            using (var stream = new FileStream(Path.Combine(pd, "Routing.cs"), FileMode.OpenOrCreate, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteAsync(template);
            }
        }
    }
}
