using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Scissorhands.Models.Settings;

namespace Scissorhands.Themes
{
    /// <summary>
    /// This represents the theme loader entity.
    /// </summary>
    public class ThemeLoader : IThemeLoader
    {
        private const string Config = "_config.json";

        private readonly WebAppSettings _settings;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeLoader"/> class.
        /// </summary>
        /// <param name="settings"><see cref="WebAppSettings"/> instance.</param>
        public ThemeLoader(WebAppSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            this._settings = settings;
        }

        /// <summary>
        /// Loads the theme configuration file.
        /// </summary>
        /// <returns>Returns the <see cref="SiteSettings"/> instance.</returns>
        public async Task<SiteSettings> LoadAsync()
        {
            var themepath = $"~/Themes/{this._settings.Theme}/{Config}";
            if (!File.Exists(themepath))
            {
                throw new InvalidOperationException("_config.json not found");
            }

            var settings = new JsonSerializerSettings
                               {
                                   ContractResolver = new CamelCasePropertyNamesContractResolver(),
                                   NullValueHandling = NullValueHandling.Ignore,
                                   MissingMemberHandling = MissingMemberHandling.Ignore
                               };

            using (var stream = new FileStream(themepath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var json = await reader.ReadToEndAsync().ConfigureAwait(false);
                var config = JsonConvert.DeserializeObject<SiteSettings>(json, settings);

                this.ApplyWebAppSettings(config);
                return config;
            }
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

        private void ApplyWebAppSettings(SiteSettings config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            config.BaseUrl = this._settings.BaseUrl;
            config.BasePath = this._settings.BasePath;
            config.Authors = this._settings.Authors;
            config.FeedTypes = this._settings.FeedTypes;
        }
    }
}