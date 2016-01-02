using System.Threading.Tasks;

using Scissorhands.Models.Settings;

namespace Scissorhands.Themes
{
    /// <summary>
    /// This provides interfaces to the theme loader class.
    /// </summary>
    public interface IThemeLoader
    {
        /// <summary>
        /// Loads the theme configuration file.
        /// </summary>
        /// <returns>Returns the <see cref="SiteSettings"/> instance.</returns>
        Task<SiteSettings> LoadAsync();
    }
}