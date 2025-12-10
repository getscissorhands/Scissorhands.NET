using ScissorHands.Core.Manifests;

namespace ScissorHands.Core.Services;

public interface IThemeService
{
    ThemeManifest LoadManifest(string themeName);
    void CopyAssets(string themeName, string destination);
}
