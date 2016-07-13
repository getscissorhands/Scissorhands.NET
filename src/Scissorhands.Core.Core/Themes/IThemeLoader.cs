using System.Threading.Tasks;

using Microsoft.Extensions.PlatformAbstractions;

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
        /// <param name="env"><see cref="IApplicationEnvironment"/> instance.</param>
        /// <returns>Returns the <see cref="ThemeConfigSettings"/> instance.</returns>
        Task<ThemeConfigSettings> LoadAsync(IApplicationEnvironment env);
    }
}