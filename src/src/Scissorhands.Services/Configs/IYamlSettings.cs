using System;
using System.Collections.Generic;

namespace Aliencube.Scissorhands.Services.Configs
{
    /// <summary>
    /// This provides interfaces to the <see cref="YamlSettings" /> class.
    /// </summary>
    public interface IYamlSettings : IDisposable
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

        /// <summary>
        /// Gets the theme base path.
        /// </summary>
        string ThemeBasePath { get; }

        /// <summary>
        /// Gets the post base path.
        /// </summary>
        string PostBasePath { get; }

        /// <summary>
        /// Gets the published base path.
        /// </summary>
        string PublishedBasePath { get; }
    }
}