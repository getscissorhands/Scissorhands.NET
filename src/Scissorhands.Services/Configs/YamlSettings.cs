using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using Aliencube.Scissorhands.Services.Interfaces;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Aliencube.Scissorhands.Services.Configs
{
    /// <summary>
    /// This represents the entity for YAML settings.
    /// </summary>
    public class YamlSettings : IYamlSettings
    {
        private string _themeBasePath;
        private string _postBasePath;
        private string _publishedBasePath;
        private bool _disposed;

        /// <summary>
        /// Initialises a new instance of the <see cref="YamlSettings" /> class.
        /// </summary>
        public YamlSettings()
        {
            this.Directories = new Directories();
            this.Themes = new List<Theme>() { new Theme() };
            this.Contents = new Contents();
        }

        /// <summary>
        /// Gets or sets the directories.
        /// </summary>
        public Directories Directories { get; set; }

        /// <summary>
        /// Gets or sets the themes.
        /// </summary>
        public List<Theme> Themes { get; set; }

        /// <summary>
        /// Gets or sets the contents.
        /// </summary>
        public Contents Contents { get; set; }

        /// <summary>
        /// Gets the theme base path.
        /// </summary>
        public string ThemeBasePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this._themeBasePath))
                {
                    this._themeBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this.Directories.Themes);
                }

                return this._themeBasePath;
            }
        }

        /// <summary>
        /// Gets the post base path.
        /// </summary>
        public string PostBasePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this._postBasePath))
                {
                    this._postBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this.Directories.Posts);
                }

                return this._postBasePath;
            }
        }

        /// <summary>
        /// Gets the published base path.
        /// </summary>
        public string PublishedBasePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this._publishedBasePath))
                {
                    this._publishedBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this.Directories.Published);
                }

                return this._publishedBasePath;
            }
        }

        /// <summary>
        /// Loads <c>config.yml</c> into <see cref="YamlSettings" />.
        /// </summary>
        /// <param name="filename">
        /// The filename. Default value is <c>config.yml</c>.
        /// </param>
        /// <returns>
        /// Returns the <see cref="YamlSettings" /> instance.
        /// </returns>
        public static YamlSettings Load(string filename = "config.yml")
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentNullException("filename");
            }

            var configpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);

            YamlSettings settings;
            using (var stream = new FileStream(configpath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream))
            {
                var deserialiser = new Deserializer(namingConvention: new CamelCaseNamingConvention());
                settings = deserialiser.Deserialize<YamlSettings>(reader);
            }

            return settings;
        }

        /// <summary>
        /// Loads <c>config.yml</c> into <see cref="YamlSettings" />.
        /// </summary>
        /// <param name="filename">
        /// The filename. Default value is <c>config.yml</c>.
        /// </param>
        /// <returns>
        /// Returns the <see cref="YamlSettings" /> instance.
        /// </returns>
        public static async Task<YamlSettings> LoadAsync(string filename = "config.yml")
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentNullException("filename");
            }

            YamlSettings settings = null;
            await Task.Run(() => { settings = Load(filename); });
            return settings;
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