using System.IO.Abstractions.TestingHelpers;

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

public class StaticSiteGeneratorRouteTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Given_PageAndDirectoryIndex_When_BuildInvoked_Then_It_Should_ReportTheCollision(bool preview)
    {
        var fileSystem = new MockFileSystem();
        var root = fileSystem.Path.GetPathRoot(Environment.CurrentDirectory) ?? fileSystem.Path.DirectorySeparatorChar.ToString();
        var baseRoot = fileSystem.Path.Combine(root, "base");
        var contentsRoot = fileSystem.Path.Combine(baseRoot, "contents");
        var themesRoot = fileSystem.Path.Combine(baseRoot, "themes");
        var pagePath = fileSystem.Path.Combine(contentsRoot, "pages", "parent.md");
        var indexPath = fileSystem.Path.Combine(contentsRoot, "pages", "parent", "index.md");
        fileSystem.AddFile(pagePath, new MockFileData("# Parent"));
        fileSystem.AddFile(indexPath, new MockFileData("# Parent index"));
        var loader = new ContentLoader(
            new TestAppPaths(baseRoot, contentsRoot, themesRoot),
            fileSystem,
            new SiteManifest(),
            Substitute.For<ILogger<ContentLoader>>());
        var documents = await loader.LoadAsync(Xunit.TestContext.Current.CancellationToken);
        var (generator, destination) = CreateGenerator(documents);

        var exception = await Should.ThrowAsync<InvalidDataException>(() =>
            generator.BuildAsync<TestMainLayout, TestIndexView, TestPostView, TestPageView, TestNotFoundView, TestTagListView, TestTagView>(
                destination, preview, Xunit.TestContext.Current.CancellationToken));

        exception.Message.ShouldContain("Output collision");
        exception.Message.ShouldContain(pagePath);
        exception.Message.ShouldContain(indexPath);
    }

    [Fact]
    public async Task Given_TraversalSlug_When_BuildInvoked_Then_It_Should_RejectTheRoute()
    {
        var document = new ContentDocument
        {
            SourcePath = "outside.md",
            Kind = ContentKind.Page,
            Metadata = new ContentMetadata { Slug = "../../outside" },
        };
        var (generator, destination) = CreateGenerator([document]);

        var exception = await Should.ThrowAsync<InvalidDataException>(() =>
            generator.BuildAsync<TestMainLayout, TestIndexView, TestPostView, TestPageView, TestNotFoundView, TestTagListView, TestTagView>(
                destination,
                preview: false,
                CancellationToken.None));

        exception.Message.ShouldContain("relative path segment");
    }

    [Fact]
    public async Task Given_DuplicateSlugs_When_BuildInvoked_Then_It_Should_ReportTheCollisionBeforeWriting()
    {
        var documents = new[]
        {
            new ContentDocument
            {
                SourcePath = "first.md",
                Kind = ContentKind.Page,
                Metadata = new ContentMetadata { Slug = "same" },
            },
            new ContentDocument
            {
                SourcePath = "second.md",
                Kind = ContentKind.Page,
                Metadata = new ContentMetadata { Slug = "same" },
            },
        };
        var (generator, destination) = CreateGenerator(documents);

        var exception = await Should.ThrowAsync<InvalidDataException>(() =>
            generator.BuildAsync<TestMainLayout, TestIndexView, TestPostView, TestPageView, TestNotFoundView, TestTagListView, TestTagView>(
                destination,
                preview: false,
                CancellationToken.None));

        exception.Message.ShouldContain("Output collision");
        exception.Message.ShouldContain("first.md");
        exception.Message.ShouldContain("second.md");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(".")]
    [InlineData("..")]
    public async Task Given_InvalidTag_When_BuildInvoked_Then_It_Should_RejectTheGeneratedTagRoute(string tag)
    {
        var document = new ContentDocument
        {
            SourcePath = "tagged.md",
            Kind = ContentKind.Post,
            Metadata = new ContentMetadata { Slug = "post", Tags = [tag] },
        };
        var (generator, destination) = CreateGenerator([document]);

        await Should.ThrowAsync<InvalidDataException>(() =>
            generator.BuildAsync<TestMainLayout, TestIndexView, TestPostView, TestPageView, TestNotFoundView, TestTagListView, TestTagView>(
                destination,
                preview: false,
                CancellationToken.None));
    }

    [Fact]
    public async Task Given_RouteNestedBelowOutputFile_When_BuildInvoked_Then_It_Should_ReportTheCollision()
    {
        var document = new ContentDocument
        {
            SourcePath = "nested.md",
            Kind = ContentKind.Page,
            Metadata = new ContentMetadata { Slug = "404.html/nested" },
        };
        var (generator, destination) = CreateGenerator([document]);

        var exception = await Should.ThrowAsync<InvalidDataException>(() =>
                generator.BuildAsync<TestMainLayout, TestIndexView, TestPostView, TestPageView, TestNotFoundView, TestTagListView, TestTagView>(
                    destination,
                    preview: false,
                    CancellationToken.None));

        exception.Message.ShouldContain("Output collision");
        exception.Message.ShouldContain("404.html");
    }

    private static (StaticSiteGenerator Generator, string Destination) CreateGenerator(IEnumerable<ContentDocument> documents)
    {
        var fileSystem = new MockFileSystem();
        var root = fileSystem.Path.GetPathRoot(Environment.CurrentDirectory) ?? fileSystem.Path.DirectorySeparatorChar.ToString();
        var baseRoot = fileSystem.Path.Combine(root, "base");
        var contentsRoot = fileSystem.Path.Combine(baseRoot, "contents");
        var themesRoot = fileSystem.Path.Combine(baseRoot, "themes");
        var destination = fileSystem.Path.Combine(root, "out");

        var contentLoader = Substitute.For<IContentLoader>();
        contentLoader.LoadAsync(Arg.Any<CancellationToken>()).Returns(documents);

        var markdownService = Substitute.For<IMarkdownService>();
        markdownService.ToHtmlAsync(Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>()).Returns(string.Empty);

        var pluginRunner = Substitute.For<IPluginRunner>();
        pluginRunner.Manifests.Returns([]);

        var themeService = Substitute.For<IThemeService>();
        themeService.LoadManifestAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new ThemeManifest());

        var generator = new StaticSiteGenerator(
            contentLoader,
            markdownService,
            pluginRunner,
            themeService,
            Substitute.For<IComponentRenderer>(),
            new TestAppPaths(baseRoot, contentsRoot, themesRoot),
            fileSystem,
            new SiteManifest(),
            Substitute.For<ILogger<StaticSiteGenerator>>());

        return (generator, destination);
    }
}
