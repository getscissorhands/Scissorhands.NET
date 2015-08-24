using System.Collections.Generic;

using Aliencube.Scissorhands.Services.Configs;

namespace Aliencube.Scissorhands.Services.Interfaces
{
    /// <summary>
    /// This provides interfaces to the <see cref="YamlSettings" /> class.
    /// </summary>
    public interface IYamlSettings
    {
        /// <summary>
        /// Gets or sets the directories.
        /// </summary>
        Directories Directories { get; set; }

        /// <summary>
        /// Gets or sets the themes.
        /// </summary>
        List<Theme> Themes { get; set; }

        /// <summary>
        /// Gets or sets the contents.
        /// </summary>
        Contents Contents { get; set; }
    }
}