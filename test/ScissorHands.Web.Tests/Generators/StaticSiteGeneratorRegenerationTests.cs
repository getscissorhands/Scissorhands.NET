using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Services;
using ScissorHands.Web.Generators;
using ScissorHands.Web.Loaders;
using ScissorHands.Web.Renderers;
using ScissorHands.Web.Runners;
using ScissorHands.Web.Tests.TestDoubles;

namespace ScissorHands.Web.Tests.Generators;

public class StaticSiteGeneratorRegenerationTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Given_CaseOnlyRename_When_Withdrawn_Then_It_Should_RemoveBothOwnedVariants(bool useMock, bool preview)
    {
        using var fixture = new Fixture(useMock ? new MockFileSystem() : null);
        await AssertCaseOnlyRenameAndWithdrawal(fixture, preview);
    }

    private static async Task AssertCaseOnlyRenameAndWithdrawal(Fixture fixture, bool preview)
    {
        fixture.Source("about");
        await fixture.Build(preview);
        fixture.Source("ABOUT");
        await fixture.Build(preview);

        fixture.Exists("ABOUT/index.html").ShouldBeTrue();
        fixture.Exists("ko-kr/ABOUT/index.html").ShouldBeTrue();
        fixture.Files.Directory.EnumerateDirectories(fixture.Destination)
            .Select(Path.GetFileName).ShouldContain("ABOUT");
        fixture.Manifest().ShouldContain("ABOUT/index.html");
        fixture.Manifest().ShouldContain("ko-kr/ABOUT/index.html");
        fixture.Manifest().ShouldNotContain("about/index.html");

        fixture.Source("ABOUT", draft: true);
        await fixture.Build(false);

        fixture.Exists("ABOUT/index.html").ShouldBeFalse();
        fixture.Exists("about/index.html").ShouldBeFalse();
        fixture.Exists("ko-kr/ABOUT/index.html").ShouldBeFalse();
        fixture.Exists("ko-kr/about/index.html").ShouldBeFalse();
        fixture.Manifest().ShouldBe(["404.html", "index.html", "ko-kr/index.html"], ignoreOrder: true);
    }

    [Theory]
    [InlineData("about", "about/index.html/details", false)]
    [InlineData("about", "about/index.html/details", true)]
    [InlineData("about/index.html/details", "about", false)]
    [InlineData("about/index.html/details", "about", true)]
    public async Task Given_RealFileDirectoryTransition_When_Regenerated_Then_It_Should_MatchACleanBuild(
        string oldRoute, string newRoute, bool preview)
    {
        using var fixture = new Fixture();
        fixture.Source(oldRoute);
        await fixture.Build(preview);
        fixture.Output("unmanaged.txt", "Keep");
        fixture.Source(newRoute);

        await fixture.Build(preview);
        var clean = Path.Combine(fixture.Root, "clean");
        await fixture.Build(preview, clean);

        fixture.Exists(newRoute + "/index.html").ShouldBeTrue();
        fixture.Exists("ko-kr/" + newRoute + "/index.html").ShouldBeTrue();
        fixture.Exists(oldRoute + "/index.html").ShouldBeFalse();
        fixture.Exists("ko-kr/" + oldRoute + "/index.html").ShouldBeFalse();
        fixture.Read("unmanaged.txt").ShouldBe("Keep");
        foreach (var relative in fixture.Manifest())
        {
            fixture.Read(relative).ShouldBe(fixture.Files.File.ReadAllText(Path.Combine(clean, relative)));
        }
    }

    [Theory]
    [InlineData("protected/index.html", "protected/index.html/details")]
    [InlineData("protected/index.html/keep.txt", "protected")]
    [InlineData("protected/index.html", "protected")]
    public async Task Given_UnmanagedBlockingOutput_When_Generated_Then_It_Should_NotDeleteOrClaimIt(string blockingFile, string newRoute)
    {
        using var fixture = new Fixture();
        fixture.Source("about");
        await fixture.Build();
        fixture.Output(blockingFile, "Unmanaged");
        fixture.Source(newRoute);

        var error = await Should.ThrowAsync<InvalidDataException>(() => fixture.Build());

        error.Message.ShouldContain("unmanaged", Case.Insensitive);
        fixture.Read(blockingFile).ShouldBe("Unmanaged");
        fixture.Manifest().ShouldNotContain(newRoute + "/index.html");
    }

    [Fact]
    public async Task Given_UnmanagedFileAmongStaleChildren_When_ReplacingDirectory_Then_It_Should_PreserveTheFileAndAllowRetry()
    {
        using var fixture = new Fixture();
        fixture.Source("about/index.html/details");
        await fixture.Build();
        fixture.Output("about/index.html/keep.txt", "Keep");
        fixture.Source("about");

        await Should.ThrowAsync<InvalidDataException>(() => fixture.Build());
        fixture.Read("about/index.html/keep.txt").ShouldBe("Keep");

        fixture.Files.File.Delete(Path.Combine(fixture.Destination, "about/index.html/keep.txt"));
        await fixture.Build();
        fixture.Exists("about/index.html").ShouldBeTrue();
        fixture.Exists("ko-kr/about/index.html").ShouldBeTrue();
    }

    [Theory]
    [InlineData("ABOUT")]
    [InlineData("about/index.html/details")]
    public async Task Given_FailedGenerationAfterReplacement_When_WithdrawnAndRetried_Then_It_Should_RemoveIncompleteOutput(string route)
    {
        using var fixture = new Fixture();
        fixture.Source("about");
        await fixture.Build();
        fixture.Source(route);
        fixture.FailRoute = "ko-kr/" + route;
        await Should.ThrowAsync<InvalidOperationException>(() => fixture.Build());
        fixture.Exists(route + "/index.html").ShouldBeTrue();
        fixture.Manifest().ShouldContain(route + "/index.html");

        fixture.FailRoute = null;
        fixture.Source(route, draft: true);
        await fixture.Build(false);

        fixture.Exists(route + "/index.html").ShouldBeFalse();
        fixture.Exists("about/index.html").ShouldBeFalse();
        fixture.Exists("ko-kr/" + route + "/index.html").ShouldBeFalse();
        fixture.Manifest().ShouldBe(["404.html", "index.html", "ko-kr/index.html"], ignoreOrder: true);
    }

    [Fact]
    public async Task Given_LinkInsideAReplacementDirectory_When_Regenerated_Then_It_Should_RejectTheLink()
    {
        using var fixture = new Fixture(new MockFileSystem());
        fixture.Source("about/index.html/details");
        await fixture.Build();
        var linked = Path.Combine(fixture.Destination, "about/index.html/linked");
        fixture.Files.Directory.CreateDirectory(linked);
        fixture.Files.File.SetAttributes(linked, FileAttributes.Directory | FileAttributes.ReparsePoint);
        fixture.Source("about");

        var error = await Should.ThrowAsync<InvalidDataException>(() => fixture.Build());

        error.Message.ShouldContain("filesystem link");
        fixture.Files.Directory.Exists(linked).ShouldBeTrue();
    }

    private sealed class Fixture : IDisposable
    {
        private readonly DirectoryInfo? _temporaryDirectory;
        private readonly ServiceProvider _provider;
        private readonly StaticSiteGenerator _generator;

        public Fixture(IFileSystem? fileSystem = null)
        {
            if (fileSystem is null)
            {
                _temporaryDirectory = Directory.CreateTempSubdirectory("scissorhands-regeneration-");
                Root = _temporaryDirectory.FullName;
                Files = new FileSystem();
            }
            else
            {
                Files = fileSystem;
                Root = Path.Combine(Path.GetPathRoot(Environment.CurrentDirectory)!, "regeneration-tests");
            }
            Destination = Path.Combine(Root, "output");
            Files.Directory.CreateDirectory(Root);
            var paths = new TestAppPaths(Root, Path.Combine(Root, "contents"), Path.Combine(Root, "themes"));
            var site = new SiteManifest
            {
                Locale = "en-us",
                LocalizationFallbackMessages = new Dictionary<string, string?> { ["ko-kr"] = "Translation unavailable." },
            };
            var theme = Substitute.For<IThemeService>();
            theme.LoadManifestAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new ThemeManifest { Slug = "default" });
            var services = new ServiceCollection().AddLogging().AddSingleton(theme);
            _provider = services.BuildServiceProvider();
            var renderer = new ComponentRenderer(_provider.GetRequiredService<IServiceScopeFactory>(), _provider.GetRequiredService<ILoggerFactory>());
            var markdown = Substitute.For<IMarkdownService>();
            markdown.ToHtmlAsync(Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>()).Returns("<p>Body</p>");
            var plugins = Substitute.For<IPluginRunner>();
            plugins.Manifests.Returns([]);
            plugins.RunPreMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>()).Returns(call => call.ArgAt<ContentDocument>(0));
            plugins.RunPostMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>()).Returns(call => call.ArgAt<ContentDocument>(0));
            plugins.RunPostHtmlAsync(Arg.Any<string>(), Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>())
                .Returns(call => call.ArgAt<ContentDocument>(1).Metadata.Slug == FailRoute
                    ? throw new InvalidOperationException("Simulated generation failure")
                    : call.ArgAt<string>(0));
            _generator = new StaticSiteGenerator(
                new ContentLoader(paths, Files, site, Substitute.For<ILogger<ContentLoader>>()),
                markdown, plugins, theme, renderer, paths, Files, site, Substitute.For<ILogger<StaticSiteGenerator>>());
        }

        public IFileSystem Files { get; }
        public string Root { get; }
        public string Destination { get; }
        public string? FailRoute { get; set; }
        public void Source(string slug, bool draft = false)
        {
            var path = Path.Combine(Root, "contents", "pages", "about.md");
            Files.Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            Files.File.WriteAllText(path, $"---\nslug: {slug}\ndraft: {draft}\n---\nBody");
        }
        public void Output(string relative, string value)
        {
            var path = Path.Combine(Destination, relative);
            Files.Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            Files.File.WriteAllText(path, value);
        }
        public bool Exists(string relative) => Files.File.Exists(Path.Combine(Destination, relative));
        public string Read(string relative) => Files.File.ReadAllText(Path.Combine(Destination, relative));
        public string[] Manifest() => JsonSerializer.Deserialize<string[]>(Read(".scissorhands-output.json"))!;
        public Task Build(bool preview = true, string? destination = null) =>
            _generator.BuildAsync<MainLayout, IndexView, PostView, PageView, NotFoundView, TagListView, TagView>(
                destination ?? Destination, preview, Xunit.TestContext.Current.CancellationToken);
        public void Dispose()
        {
            _provider.Dispose();
            _temporaryDirectory?.Delete(recursive: true);
        }
    }
}
