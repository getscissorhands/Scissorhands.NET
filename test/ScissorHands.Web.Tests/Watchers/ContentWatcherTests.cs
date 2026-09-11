using Microsoft.Extensions.Logging;

using ScissorHands.Web.Watchers;

namespace ScissorHands.Web.Tests.Watchers;

public class ContentWatcherTests
{
    [Fact]
    public void Given_MissingWatchDirectories_When_Constructed_Then_It_Should_CreateThem()
    {
        var root = Path.Combine(Path.GetTempPath(), $"scissorhands-watcher-{Guid.NewGuid():N}");
        var contentRoot = Path.Combine(root, "contents");
        var themeRoot = Path.Combine(root, "themes");

        try
        {
            using var watcher = new ContentWatcher(
                contentRoot,
                themeRoot,
                TimeSpan.FromMilliseconds(10),
                () => Task.CompletedTask,
                Substitute.For<ILogger<ContentWatcher>>());

            Directory.Exists(contentRoot).ShouldBeTrue();
            Directory.Exists(themeRoot).ShouldBeTrue();
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }
}
