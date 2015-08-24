using System.Collections.Generic;
using System.IO;
using System.Reflection;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Aliencube.Scissorhands.Services.Configs
{
    /// <summary>
    /// This represents the entity for YAML settings.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Settings" /> class.
        /// </summary>
        public Settings()
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
        /// Loads <c>config.yml</c> into <see cref="Settings" />.
        /// </summary>
        /// <returns>
        /// Returns the <see cref="Settings" /> instance.
        /// </returns>
        public static Settings Load()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var configpath = string.Format(@"{0}\{1}", assembly.Location, "config.yml");

            Settings settings;
            using (var stream = new FileStream(configpath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream))
            {
                var deserialiser = new Deserializer(namingConvention: new CamelCaseNamingConvention());
                settings = deserialiser.Deserialize<Settings>(reader);
            }

            return settings;
        }
    }
}