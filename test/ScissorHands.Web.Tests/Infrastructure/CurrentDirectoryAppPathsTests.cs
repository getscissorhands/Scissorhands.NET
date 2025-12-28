using ScissorHands.Core.Manifests;
using ScissorHands.Web.Infrastructure;

namespace ScissorHands.Web.Tests.Infrastructure;

[Collection("NonParallel")]
public class CurrentDirectoryAppPathsTests
{
    [Fact]
    public void Given_CurrentDirectory_When_AppPathsInvoked_Then_It_Should_ReturnExpectedRoots()
    {
        var original = Directory.GetCurrentDirectory();
        var temp = Path.Combine(Path.GetTempPath(), "ScissorHands.Web.Tests", Guid.NewGuid().ToString("N"));

        try
        {
            Directory.CreateDirectory(temp);
            Directory.SetCurrentDirectory(temp);

            var expectedBasePath = Directory.GetCurrentDirectory();

            var paths = new CurrentDirectoryAppPaths();

            paths.BasePath.ShouldBe(expectedBasePath);

            paths.GetContentsRoot().ShouldBe(Path.GetFullPath(Path.Combine(expectedBasePath, SiteManifest.CONTENTS_DIRECTORY)));
            paths.GetThemesRoot().ShouldBe(Path.GetFullPath(Path.Combine(expectedBasePath, ThemeManifest.THEME_DIRECTORY)));
        }
        finally
        {
            Directory.SetCurrentDirectory(original);

            if (Directory.Exists(temp))
            {
                Directory.Delete(temp, recursive: true);
            }
        }
    }
}
