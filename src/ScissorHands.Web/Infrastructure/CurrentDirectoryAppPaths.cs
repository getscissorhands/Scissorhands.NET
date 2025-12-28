using ScissorHands.Core.Manifests;
using ScissorHands.Web.Abstractions;

namespace ScissorHands.Web.Infrastructure;

/// <summary>
/// This represents the application paths entity for current directory.
/// </summary>
public sealed class CurrentDirectoryAppPaths : IAppPaths
{
    /// <inheritdoc />
    public string BasePath => Directory.GetCurrentDirectory();

    /// <inheritdoc />
    public string GetContentsRoot()
        => Path.GetFullPath(Path.Combine(BasePath, SiteManifest.CONTENTS_DIRECTORY));

    /// <inheritdoc />
    public string GetThemesRoot()
        => Path.GetFullPath(Path.Combine(BasePath, ThemeManifest.THEME_DIRECTORY));
}
