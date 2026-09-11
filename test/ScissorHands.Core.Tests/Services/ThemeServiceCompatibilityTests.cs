using ScissorHands.Core.Manifests;
using ScissorHands.Core.Services;

namespace ScissorHands.Core.Tests.Services;

public class ThemeServiceCompatibilityTests
{
    [Fact]
    public async Task Given_LegacyThemeServiceImplementation_When_CancellableOverloadInvoked_Then_It_Should_Delegate()
    {
        IThemeService service = new LegacyThemeService();

        var manifest = await service.LoadManifestAsync("legacy", CancellationToken.None);
        await service.CopyAssetsAsync("legacy", "output", CancellationToken.None);

        manifest.Slug.ShouldBe("legacy");
    }

    [Fact]
    public async Task Given_LegacyThemeServiceImplementation_When_TokenCancelled_Then_It_Should_Not_InvokeLegacyMethod()
    {
        IThemeService service = new LegacyThemeService();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Should.ThrowAsync<OperationCanceledException>(() =>
            service.LoadManifestAsync("legacy", cancellation.Token));
    }

    private sealed class LegacyThemeService : IThemeService
    {
        public Task<ThemeManifest> LoadManifestAsync(string themeSlug)
            => Task.FromResult(new ThemeManifest { Slug = themeSlug });

        public Task CopyAssetsAsync(string themeSlug, string destination)
            => Task.CompletedTask;
    }
}
