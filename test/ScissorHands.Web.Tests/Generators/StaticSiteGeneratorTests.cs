using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Services;
using ScissorHands.Web.Generators;
using ScissorHands.Web.Loaders;
using ScissorHands.Web.Renderers;
using ScissorHands.Web.Runners;
using ScissorHands.Web.Tests.TestDoubles;

using System.IO.Abstractions.TestingHelpers;

namespace ScissorHands.Web.Tests.Generators;

public class StaticSiteGeneratorTests
{
    [Fact]
    public async Task Given_ContentDocuments_When_BuildAsync_Invoked_Then_It_Should_WriteIndexAndDocumentHtmlFiles()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var root = fileSystem.Path.GetPathRoot(Environment.CurrentDirectory) ?? fileSystem.Path.DirectorySeparatorChar.ToString();
        var baseRoot = fileSystem.Path.Combine(root, "base");
        var contentsRoot = fileSystem.Path.Combine(baseRoot, "contents");
        var themesRoot = fileSystem.Path.Combine(baseRoot, "themes");
        var destination = fileSystem.Path.Combine(root, "out");

        var paths = new TestAppPaths(basePath: baseRoot, contentsRoot, themesRoot);

        var site = new SiteManifest
        {
            Title = "My Site",
            Description = "My Description",
            Theme = "minimal"
        };

        var post = new ContentDocument
        {
            Kind = ContentKind.Post,
            Markdown = "# Post",
            Metadata = new ContentMetadata { Title = "Post", Slug = "blog/post-1" }
        };

        var page = new ContentDocument
        {
            Kind = ContentKind.Page,
            Markdown = "# About",
            Metadata = new ContentMetadata { Title = "About", Slug = "about" }
        };

        var contentLoader = Substitute.For<IContentLoader>();
        contentLoader
            .LoadAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<ContentDocument>>(new[] { post, page }));

        var markdownService = Substitute.For<IMarkdownService>();
        markdownService
            .ToHtmlAsync(Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult($"HTML:{callInfo.ArgAt<string>(0)}"));

        var pluginRunner = Substitute.For<IPluginRunner>();
        pluginRunner.Manifests.Returns(new List<PluginManifest> { new() { Name = "Plugin" } });

        pluginRunner
            .RunPreMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.ArgAt<ContentDocument>(0)));

        pluginRunner
            .RunPostMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.ArgAt<ContentDocument>(0)));

        pluginRunner
            .RunPostHtmlAsync(Arg.Any<string>(), Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult($"FINAL:{callInfo.ArgAt<string>(0)}"));

        var themeService = Substitute.For<IThemeService>();
        themeService
            .LoadManifestAsync(Arg.Any<string>())
            .Returns(Task.FromResult(new ThemeManifest { Name = "Minimal", Slug = "minimal" }));

        themeService
            .CopyAssetsAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.CompletedTask);

        var renderer = Substitute.For<IComponentRenderer>();
        renderer
            .RenderAsync<TestIndexView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("INDEX"));

        renderer
            .RenderAsync<TestPostView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("POST"));

        renderer
            .RenderAsync<TestPageView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("PAGE"));

        renderer
            .RenderAsync<TestNotFoundView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("NOTFOUND"));

        var logger = Substitute.For<ILogger<StaticSiteGenerator>>();

        var generator = new StaticSiteGenerator(
            contentLoader,
            markdownService,
            pluginRunner,
            themeService,
            renderer,
            paths,
            fileSystem,
            site,
            logger);

        // Act
        await generator.BuildAsync<TestMainLayout, TestIndexView, TestPostView, TestPageView, TestNotFoundView>(destination, preview: false, CancellationToken.None);

        // Assert
        site.DescriptionInHtml.ShouldBe("HTML:My Description");

        fileSystem.File.Exists(fileSystem.Path.Combine(destination, "index.html")).ShouldBeTrue();
        fileSystem.File.ReadAllText(fileSystem.Path.Combine(destination, "index.html")).ShouldBe("FINAL:INDEX");

        fileSystem.File.Exists(fileSystem.Path.Combine(destination, "blog", "post-1", "index.html")).ShouldBeTrue();
        fileSystem.File.ReadAllText(fileSystem.Path.Combine(destination, "blog", "post-1", "index.html")).ShouldBe("FINAL:POST");

        fileSystem.File.Exists(fileSystem.Path.Combine(destination, "about", "index.html")).ShouldBeTrue();
        fileSystem.File.ReadAllText(fileSystem.Path.Combine(destination, "about", "index.html")).ShouldBe("FINAL:PAGE");

        fileSystem.File.Exists(fileSystem.Path.Combine(destination, "404.html")).ShouldBeTrue();
        fileSystem.File.ReadAllText(fileSystem.Path.Combine(destination, "404.html")).ShouldBe("FINAL:NOTFOUND");
    }

    [Fact]
    public async Task Given_404MarkdownPage_When_BuildAsync_Invoked_Then_It_Should_PassItToNotFoundView_And_NotRenderAsNormalPage()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var root = fileSystem.Path.GetPathRoot(Environment.CurrentDirectory) ?? fileSystem.Path.DirectorySeparatorChar.ToString();
        var baseRoot = fileSystem.Path.Combine(root, "base");
        var contentsRoot = fileSystem.Path.Combine(baseRoot, "contents");
        var themesRoot = fileSystem.Path.Combine(baseRoot, "themes");
        var destination = fileSystem.Path.Combine(root, "out");

        var paths = new TestAppPaths(basePath: baseRoot, contentsRoot, themesRoot);

        var site = new SiteManifest
        {
            Title = "My Site",
            Description = "My Description",
            Theme = "minimal"
        };

        var notFoundPage = new ContentDocument
        {
            Kind = ContentKind.Page,
            Markdown = "# Custom 404",
            Metadata = new ContentMetadata { Title = "Custom 404", Slug = "404.html" }
        };

        var about = new ContentDocument
        {
            Kind = ContentKind.Page,
            Markdown = "# About",
            Metadata = new ContentMetadata { Title = "About", Slug = "about" }
        };

        var contentLoader = Substitute.For<IContentLoader>();
        contentLoader
            .LoadAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<ContentDocument>>(new[] { notFoundPage, about }));

        var markdownService = Substitute.For<IMarkdownService>();
        markdownService
            .ToHtmlAsync(Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult($"HTML:{callInfo.ArgAt<string>(0)}"));

        var pluginRunner = Substitute.For<IPluginRunner>();
        pluginRunner.Manifests.Returns([]);
        pluginRunner
            .RunPreMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.ArgAt<ContentDocument>(0)));
        pluginRunner
            .RunPostMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.ArgAt<ContentDocument>(0)));
        pluginRunner
            .RunPostHtmlAsync(Arg.Any<string>(), Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult($"FINAL:{callInfo.ArgAt<string>(0)}"));

        var themeService = Substitute.For<IThemeService>();
        themeService
            .LoadManifestAsync(Arg.Any<string>())
            .Returns(Task.FromResult(new ThemeManifest { Name = "Minimal", Slug = "minimal" }));
        themeService
            .CopyAssetsAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.CompletedTask);

        IDictionary<string, object?>? capturedNotFoundParams = null;

        var renderer = Substitute.For<IComponentRenderer>();
        renderer
            .RenderAsync<TestIndexView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("INDEX"));
        renderer
            .RenderAsync<TestPageView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("PAGE"));
        renderer
            .RenderAsync<TestNotFoundView>(Arg.Any<Type>(), Arg.Do<IDictionary<string, object?>>(p => capturedNotFoundParams = p), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("NOTFOUND"));

        var logger = Substitute.For<ILogger<StaticSiteGenerator>>();

        var generator = new StaticSiteGenerator(
            contentLoader,
            markdownService,
            pluginRunner,
            themeService,
            renderer,
            paths,
            fileSystem,
            site,
            logger);

        // Act
        await generator.BuildAsync<TestMainLayout, TestIndexView, TestPostView, TestPageView, TestNotFoundView>(destination, preview: false, CancellationToken.None);

        // Assert
        capturedNotFoundParams.ShouldNotBeNull();
        capturedNotFoundParams!.ContainsKey("Document").ShouldBeTrue();

        var passedDocument = capturedNotFoundParams["Document"]!.ShouldBeAssignableTo<ContentDocument>();
        passedDocument.Metadata.Slug.ShouldBe("404.html");
        passedDocument.Html.ShouldBe("HTML:# Custom 404");

        fileSystem.File.Exists(fileSystem.Path.Combine(destination, "404.html")).ShouldBeTrue();
        fileSystem.File.ReadAllText(fileSystem.Path.Combine(destination, "404.html")).ShouldBe("FINAL:NOTFOUND");

        fileSystem.File.Exists(fileSystem.Path.Combine(destination, "404.html", "index.html")).ShouldBeFalse();

        fileSystem.File.Exists(fileSystem.Path.Combine(destination, "about", "index.html")).ShouldBeTrue();
        fileSystem.File.ReadAllText(fileSystem.Path.Combine(destination, "about", "index.html")).ShouldBe("FINAL:PAGE");
    }

    [Fact]
    public async Task Given_NoContentImagesFolder_When_BuildAsync_Invoked_Then_It_Should_NotCreateImagesOutputFolder()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var root = fileSystem.Path.GetPathRoot(Environment.CurrentDirectory) ?? fileSystem.Path.DirectorySeparatorChar.ToString();
        var baseRoot = fileSystem.Path.Combine(root, "base");
        var contentsRoot = fileSystem.Path.Combine(baseRoot, "contents");
        var themesRoot = fileSystem.Path.Combine(baseRoot, "themes");
        var destination = fileSystem.Path.Combine(root, "out");

        var paths = new TestAppPaths(basePath: baseRoot, contentsRoot, themesRoot);

        var site = new SiteManifest
        {
            Title = "My Site",
            Description = "My Description",
            Theme = "minimal"
        };

        var contentLoader = Substitute.For<IContentLoader>();
        contentLoader
            .LoadAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<ContentDocument>>([]));

        var markdownService = Substitute.For<IMarkdownService>();
        markdownService
            .ToHtmlAsync(Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult($"HTML:{callInfo.ArgAt<string>(0)}"));

        var pluginRunner = Substitute.For<IPluginRunner>();
        pluginRunner.Manifests.Returns([]);
        pluginRunner
            .RunPreMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.ArgAt<ContentDocument>(0)));
        pluginRunner
            .RunPostMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.ArgAt<ContentDocument>(0)));
        pluginRunner
            .RunPostHtmlAsync(Arg.Any<string>(), Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.ArgAt<string>(0)));

        var themeService = Substitute.For<IThemeService>();
        themeService
            .LoadManifestAsync(Arg.Any<string>())
            .Returns(Task.FromResult(new ThemeManifest { Name = "Minimal", Slug = "minimal" }));
        themeService
            .CopyAssetsAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.CompletedTask);

        var renderer = Substitute.For<IComponentRenderer>();
        renderer
            .RenderAsync<TestIndexView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("INDEX"));

        renderer
            .RenderAsync<TestNotFoundView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("NOTFOUND"));

        var logger = Substitute.For<ILogger<StaticSiteGenerator>>();

        var generator = new StaticSiteGenerator(
            contentLoader,
            markdownService,
            pluginRunner,
            themeService,
            renderer,
            paths,
            fileSystem,
            site,
            logger);

        // Act
        await generator.BuildAsync<TestMainLayout, TestIndexView, TestPostView, TestPageView, TestNotFoundView>(destination, preview: false, CancellationToken.None);

        // Assert
        fileSystem.Directory.Exists(fileSystem.Path.Combine(destination, "images")).ShouldBeFalse();
    }

    [Fact]
    public async Task Given_ContentImages_When_BuildAsync_Invoked_Then_It_Should_CopyImagesRecursively()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var root = fileSystem.Path.GetPathRoot(Environment.CurrentDirectory) ?? fileSystem.Path.DirectorySeparatorChar.ToString();
        var baseRoot = fileSystem.Path.Combine(root, "base");
        var contentsRoot = fileSystem.Path.Combine(baseRoot, "contents");
        var themesRoot = fileSystem.Path.Combine(baseRoot, "themes");
        var destination = fileSystem.Path.Combine(root, "out");

        fileSystem.AddFile(fileSystem.Path.Combine(contentsRoot, "images", "a.png"), new MockFileData("A"));
        fileSystem.AddFile(fileSystem.Path.Combine(contentsRoot, "images", "nested", "b.png"), new MockFileData("B"));

        var paths = new TestAppPaths(basePath: baseRoot, contentsRoot, themesRoot);

        var site = new SiteManifest
        {
            Title = "My Site",
            Description = "My Description",
            Theme = "minimal"
        };

        var contentLoader = Substitute.For<IContentLoader>();
        contentLoader
            .LoadAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<ContentDocument>>([]));

        var markdownService = Substitute.For<IMarkdownService>();
        markdownService
            .ToHtmlAsync(Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult($"HTML:{callInfo.ArgAt<string>(0)}"));

        var pluginRunner = Substitute.For<IPluginRunner>();
        pluginRunner.Manifests.Returns([]);
        pluginRunner
            .RunPreMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.ArgAt<ContentDocument>(0)));
        pluginRunner
            .RunPostMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.ArgAt<ContentDocument>(0)));
        pluginRunner
            .RunPostHtmlAsync(Arg.Any<string>(), Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.ArgAt<string>(0)));

        var themeService = Substitute.For<IThemeService>();
        themeService
            .LoadManifestAsync(Arg.Any<string>())
            .Returns(Task.FromResult(new ThemeManifest { Name = "Minimal", Slug = "minimal" }));
        themeService
            .CopyAssetsAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.CompletedTask);

        var renderer = Substitute.For<IComponentRenderer>();
        renderer
            .RenderAsync<TestIndexView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("INDEX"));

        renderer
            .RenderAsync<TestNotFoundView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("NOTFOUND"));

        var logger = Substitute.For<ILogger<StaticSiteGenerator>>();

        var generator = new StaticSiteGenerator(
            contentLoader,
            markdownService,
            pluginRunner,
            themeService,
            renderer,
            paths,
            fileSystem,
            site,
            logger);

        // Act
        await generator.BuildAsync<TestMainLayout, TestIndexView, TestPostView, TestPageView, TestNotFoundView>(destination, preview: false, CancellationToken.None);

        // Assert
        fileSystem.File.Exists(fileSystem.Path.Combine(destination, "images", "a.png")).ShouldBeTrue();
        fileSystem.File.Exists(fileSystem.Path.Combine(destination, "images", "nested", "b.png")).ShouldBeTrue();
    }

    [Fact]
    public async Task Given_PostsWithDifferentPublishDates_When_BuildAsync_Invoked_Then_It_Should_RenderIndexWithPostsSortedDescending()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var root = fileSystem.Path.GetPathRoot(Environment.CurrentDirectory) ?? fileSystem.Path.DirectorySeparatorChar.ToString();
        var baseRoot = fileSystem.Path.Combine(root, "base");
        var contentsRoot = fileSystem.Path.Combine(baseRoot, "contents");
        var themesRoot = fileSystem.Path.Combine(baseRoot, "themes");
        var destination = fileSystem.Path.Combine(root, "out");

        var paths = new TestAppPaths(basePath: baseRoot, contentsRoot, themesRoot);

        var site = new SiteManifest
        {
            Title = "My Site",
            Description = "My Description",
            Theme = "minimal"
        };

        var olderPost = new ContentDocument
        {
            Kind = ContentKind.Post,
            Markdown = "# Older",
            Metadata = new ContentMetadata { Title = "Older", Slug = "blog/older", Published = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero) }
        };

        var newerPost = new ContentDocument
        {
            Kind = ContentKind.Post,
            Markdown = "# Newer",
            Metadata = new ContentMetadata { Title = "Newer", Slug = "blog/newer", Published = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero) }
        };

        var page = new ContentDocument
        {
            Kind = ContentKind.Page,
            Markdown = "# About",
            Metadata = new ContentMetadata { Title = "About", Slug = "about" }
        };

        var contentLoader = Substitute.For<IContentLoader>();
        contentLoader
            .LoadAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<ContentDocument>>(new[] { olderPost, page, newerPost }));

        var markdownService = Substitute.For<IMarkdownService>();
        markdownService
            .ToHtmlAsync(Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult($"HTML:{callInfo.ArgAt<string>(0)}"));

        var pluginRunner = Substitute.For<IPluginRunner>();
        pluginRunner.Manifests.Returns([]);
        pluginRunner
            .RunPreMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.ArgAt<ContentDocument>(0)));
        pluginRunner
            .RunPostMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.ArgAt<ContentDocument>(0)));
        pluginRunner
            .RunPostHtmlAsync(Arg.Any<string>(), Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.ArgAt<string>(0)));

        var themeService = Substitute.For<IThemeService>();
        themeService
            .LoadManifestAsync(Arg.Any<string>())
            .Returns(Task.FromResult(new ThemeManifest { Name = "Minimal", Slug = "minimal" }));
        themeService
            .CopyAssetsAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.CompletedTask);

        IDictionary<string, object?>? captured = null;

        var renderer = Substitute.For<IComponentRenderer>();
        renderer
            .RenderAsync<TestIndexView>(Arg.Any<Type>(), Arg.Do<IDictionary<string, object?>>(p => captured = p), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("INDEX"));

        renderer
            .RenderAsync<TestNotFoundView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("NOTFOUND"));

        var logger = Substitute.For<ILogger<StaticSiteGenerator>>();

        var generator = new StaticSiteGenerator(
            contentLoader,
            markdownService,
            pluginRunner,
            themeService,
            renderer,
            paths,
            fileSystem,
            site,
            logger);

        // Act
        await generator.BuildAsync<TestMainLayout, TestIndexView, TestPostView, TestPageView, TestNotFoundView>(destination, preview: false, CancellationToken.None);

        // Assert
        captured.ShouldNotBeNull();
        captured!.ContainsKey("Documents").ShouldBeTrue();

        captured["Documents"].ShouldNotBeNull();
        var docs = captured["Documents"]!.ShouldBeAssignableTo<IEnumerable<ContentDocument>>().ToList();
        docs.Count.ShouldBe(2);
        docs[0].Metadata.Slug.ShouldBe("blog/newer");
        docs[1].Metadata.Slug.ShouldBe("blog/older");
    }
}

internal sealed class TestMainLayout : ScissorHands.Theme.MainLayoutBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
    }
}

internal sealed class TestIndexView : ScissorHands.Theme.IndexViewBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
    }
}

internal sealed class TestPostView : ScissorHands.Theme.PostViewBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
    }
}

internal sealed class TestPageView : ScissorHands.Theme.PageViewBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
    }
}

internal sealed class TestNotFoundView : ScissorHands.Theme.NotFoundViewBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
    }
}
