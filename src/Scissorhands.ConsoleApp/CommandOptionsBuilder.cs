using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

using Scissorhands.Extensions;

namespace Scissorhands
{
    /// <summary>
    /// This represents the builder entity for command options.
    /// </summary>
    public class CommandOptionsBuilder : ICommandOptionsBuilder
    {
        private readonly string _filepath;
        private readonly JsonSerializerSettings _settings;

        /// <summary>
        /// Initialises a new instance of the <see cref="CommandOptionsBuilder"/> class.
        /// </summary>
        /// <param name="filepath"></param>
        public CommandOptionsBuilder(string filepath = "appsettings.json")
        {
            if (string.IsNullOrWhiteSpace(filepath))
            {
                this._filepath = "appsettings.json";
            }

            this._settings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = {new StringEnumConverter()},
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
            };
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CommandOptionsBuilder"/> class with default settings.
        /// </summary>
        public static ICommandOptionsBuilder Default = new CommandOptionsBuilder();

        /// <summary>
        /// Builds options from command arguments.
        /// </summary>
        /// <param name="args">List of arguments.</param>
        /// <returns>Returns the <see cref="CommandOptions"/> built</returns>
        /// <remarks>Options are read from the <c>appsettings.json</c> first, then overridden by command line arguments.</remarks>
        public async Task<CommandOptions> BuildAsync(string[] args)
        {
            if (args.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(args));
            }

            var options = await this.GetDefaultOptionsAsyc(this._filepath).ConfigureAwait(false);

            var source =
                args.SingleOrDefault(
                    p => p.ToLowerInvariant().StartsWith("-s:") || p.ToLowerInvariant().StartsWith("--source:"));
            if (!string.IsNullOrWhiteSpace(source))
            {
                options.Source = source.Replace("-s:", "").Replace("--source:", "");
            }

            var theme =
                args.SingleOrDefault(
                    p => p.ToLowerInvariant().StartsWith("-t:") || p.ToLowerInvariant().StartsWith("--theme:"));
            if (!string.IsNullOrWhiteSpace(theme))
            {
                options.Theme = theme.Replace("-t:", "").Replace("--theme:", "");
            }

            var output =
                args.SingleOrDefault(
                    p => p.ToLowerInvariant().StartsWith("-o:") || p.ToLowerInvariant().StartsWith("--output:"));
            if (!string.IsNullOrWhiteSpace(output))
            {
                options.OutputDirectory = output.Replace("-o:", "").Replace("--output:", "");
            }

            return options;
        }

        /// <summary>
        /// Gets the default options by reading <c>appsettings.json</c>.
        /// </summary>
        /// <param name="filepath">File path of the <c>appsettings.json</c>.</param>
        /// <returns>Returns the <see cref="CommandOptions"/> deserialised.</returns>
        public async Task<CommandOptions> GetDefaultOptionsAsyc(string filepath)
        {
            if (string.IsNullOrWhiteSpace(filepath))
            {
                return new CommandOptions();
            }

            if (!File.Exists(filepath))
            {
                return new CommandOptions();
            }

            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var json = await reader.ReadToEndAsync().ConfigureAwait(false);
                var options = JsonConvert.DeserializeObject<AppSettings>(json, this._settings)?.CommandOptions;
                return options;
            }
        }
    }
}