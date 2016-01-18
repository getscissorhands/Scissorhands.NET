using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.PlatformAbstractions;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Scissorhands.Helpers;
using Scissorhands.Models.Settings;

namespace Scissorhands.Themes
{
    /// <summary>
    /// This represents the theme loader entity.
    /// </summary>
    public class ThemeLoader : IThemeLoader
    {
        private const string Config = "_config.json";

        private readonly SiteMetadataSettings _metadata;
        private readonly IFileHelper _fileHelper;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeLoader"/> class.
        /// </summary>
        /// <param name="metadata"><see cref="SiteMetadataSettings"/> instance.</param>
        /// <param name="fileHelper"><see cref="IFileHelper"/> instance.</param>
        public ThemeLoader(SiteMetadataSettings metadata, IFileHelper fileHelper)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            this._metadata = metadata;

            if (fileHelper == null)
            {
                throw new ArgumentNullException(nameof(fileHelper));
            }

            this._fileHelper = fileHelper;
        }

        /// <summary>
        /// Loads the theme configuration file.
        /// </summary>
        /// <returns>Returns the <see cref="SiteMetadataSettings"/> instance.</returns>
        public async Task<ThemeConfigSettings> LoadAsync(IApplicationEnvironment env)
        {
            if (env == null)
            {
                throw new ArgumentNullException(nameof(env));
            }

            var configpath = Path.Combine(env.ApplicationBasePath, $"Themes/{this._metadata.Theme}/{Config}");
            if (!File.Exists(configpath))
            {
                throw new InvalidOperationException("_config.json not found");
            }

            var settings = new JsonSerializerSettings
                               {
                                   ContractResolver = new CamelCasePropertyNamesContractResolver(),
                                   NullValueHandling = NullValueHandling.Ignore,
                                   MissingMemberHandling = MissingMemberHandling.Ignore
                               };

            var json = await this._fileHelper.ReadAsync(configpath).ConfigureAwait(false);
            var config = JsonConvert.DeserializeObject<ThemeConfigSettings>(json, settings);
            return config;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this._disposed = true;
        }
    }
}