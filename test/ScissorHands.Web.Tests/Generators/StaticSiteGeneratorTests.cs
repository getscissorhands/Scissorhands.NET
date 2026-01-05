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

        renderer
            .RenderAsync<TestTagListView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("TAGLIST"));

        renderer
            .RenderAsync<TestTagView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("TAG"));

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
        await generator.BuildAsync<TestMainLayout, TestIndexView, TestPostView, TestPageView, TestNotFoundView, TestTagListView, TestTagView>(destination, preview: false, CancellationToken.None);

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
        renderer
            .RenderAsync<TestTagListView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("TAGLIST"));
        renderer
            .RenderAsync<TestTagView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("TAG"));

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
        await generator.BuildAsync<TestMainLayout, TestIndexView, TestPostView, TestPageView, TestNotFoundView, TestTagListView, TestTagView>(destination, preview: false, CancellationToken.None);

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

        renderer
            .RenderAsync<TestTagListView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("TAGLIST"));

        renderer
            .RenderAsync<TestTagView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("TAG"));

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
        await generator.BuildAsync<TestMainLayout, TestIndexView, TestPostView, TestPageView, TestNotFoundView, TestTagListView, TestTagView>(destination, preview: false, CancellationToken.None);

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

        renderer
            .RenderAsync<TestTagListView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("TAGLIST"));

        renderer
            .RenderAsync<TestTagView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("TAG"));

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
        await generator.BuildAsync<TestMainLayout, TestIndexView, TestPostView, TestPageView, TestNotFoundView, TestTagListView, TestTagView>(destination, preview: false, CancellationToken.None);

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

        renderer
            .RenderAsync<TestTagListView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("TAGLIST"));

        renderer
            .RenderAsync<TestTagView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("TAG"));

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
        await generator.BuildAsync<TestMainLayout, TestIndexView, TestPostView, TestPageView, TestNotFoundView, TestTagListView, TestTagView>(destination, preview: false, CancellationToken.None);

        // Assert
        captured.ShouldNotBeNull();
        captured!.ContainsKey("Documents").ShouldBeTrue();

        captured["Documents"].ShouldNotBeNull();
        var docs = captured["Documents"]!.ShouldBeAssignableTo<IEnumerable<ContentDocument>>().ToList();
        docs.Count.ShouldBe(2);
        docs[0].Metadata.Slug.ShouldBe("blog/newer");
        docs[1].Metadata.Slug.ShouldBe("blog/older");
    }

    [Fact]
    public async Task Given_DocumentsWithTags_When_BuildAsync_Invoked_Then_It_Should_GenerateTagListAndTagPages()
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

        var post1 = new ContentDocument
        {
            Kind = ContentKind.Post,
            Markdown = "# Post 1",
            Metadata = new ContentMetadata
            {
                Title = "Hello World",
                Slug = "blog/hello-world",
                Published = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
                Tags = ["blazor", "azure"]
            }
        };

        var post2 = new ContentDocument
        {
            Kind = ContentKind.Post,
            Markdown = "# Post 2",
            Metadata = new ContentMetadata
            {
                Title = "Lorem Ipsum",
                Slug = "blog/lorem-ipsum",
                Published = new DateTimeOffset(2026, 1, 3, 0, 0, 0, TimeSpan.Zero),
                Tags = ["asp-net", "azure"]
            }
        };

        var page1 = new ContentDocument
        {
            Kind = ContentKind.Page,
            Markdown = "# Getting Started",
            Metadata = new ContentMetadata
            {
                Title = "Getting Started",
                Slug = "getting-started",
                Tags = ["blazor"]
            }
        };

        var contentLoader = Substitute.For<IContentLoader>();
        contentLoader
            .LoadAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<ContentDocument>>(new[] { post1, post2, page1 }));

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
        renderer
            .RenderAsync<TestTagListView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("TAGLIST"));
        renderer
            .RenderAsync<TestTagView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("TAG"));

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
        await generator.BuildAsync<TestMainLayout, TestIndexView, TestPostView, TestPageView, TestNotFoundView, TestTagListView, TestTagView>(destination, preview: false, CancellationToken.None);

        // Assert - tag list page at /tags
        fileSystem.File.Exists(fileSystem.Path.Combine(destination, "tags", "index.html")).ShouldBeTrue();
        fileSystem.File.ReadAllText(fileSystem.Path.Combine(destination, "tags", "index.html")).ShouldBe("FINAL:TAGLIST");

        // Assert - individual tag pages
        fileSystem.File.Exists(fileSystem.Path.Combine(destination, "tags", "blazor", "index.html")).ShouldBeTrue();
        fileSystem.File.ReadAllText(fileSystem.Path.Combine(destination, "tags", "blazor", "index.html")).ShouldBe("FINAL:TAG");

        fileSystem.File.Exists(fileSystem.Path.Combine(destination, "tags", "azure", "index.html")).ShouldBeTrue();
        fileSystem.File.ReadAllText(fileSystem.Path.Combine(destination, "tags", "azure", "index.html")).ShouldBe("FINAL:TAG");

        fileSystem.File.Exists(fileSystem.Path.Combine(destination, "tags", "asp-net", "index.html")).ShouldBeTrue();
        fileSystem.File.ReadAllText(fileSystem.Path.Combine(destination, "tags", "asp-net", "index.html")).ShouldBe("FINAL:TAG");
    }

    [Fact]
    public async Task Given_DocumentsWithTags_When_BuildAsync_Invoked_Then_TagListView_Should_ReceiveCorrectParameters()
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

        var post1 = new ContentDocument
        {
            Kind = ContentKind.Post,
            Markdown = "# Post 1",
            Metadata = new ContentMetadata
            {
                Title = "Hello World",
                Slug = "blog/hello-world",
                Published = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
                Tags = ["blazor", "azure"]
            }
        };

        var post2 = new ContentDocument
        {
            Kind = ContentKind.Post,
            Markdown = "# Post 2",
            Metadata = new ContentMetadata
            {
                Title = "Lorem Ipsum",
                Slug = "blog/lorem-ipsum",
                Published = new DateTimeOffset(2026, 1, 3, 0, 0, 0, TimeSpan.Zero),
                Tags = ["asp-net", "azure"]
            }
        };

        var page1 = new ContentDocument
        {
            Kind = ContentKind.Page,
            Markdown = "# Getting Started",
            Metadata = new ContentMetadata
            {
                Title = "Getting Started",
                Slug = "getting-started",
                Tags = ["blazor"]
            }
        };

        var contentLoader = Substitute.For<IContentLoader>();
        contentLoader
            .LoadAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<ContentDocument>>(new[] { post1, post2, page1 }));

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

        IDictionary<string, object?>? capturedTagListParams = null;

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
        renderer
            .RenderAsync<TestTagListView>(Arg.Any<Type>(), Arg.Do<IDictionary<string, object?>>(p => capturedTagListParams = p), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("TAGLIST"));
        renderer
            .RenderAsync<TestTagView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("TAG"));

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
        await generator.BuildAsync<TestMainLayout, TestIndexView, TestPostView, TestPageView, TestNotFoundView, TestTagListView, TestTagView>(destination, preview: false, CancellationToken.None);

        // Assert
        capturedTagListParams.ShouldNotBeNull();
        capturedTagListParams!.ContainsKey("TaggedDocuments").ShouldBeTrue();

        var taggedDocs = capturedTagListParams["TaggedDocuments"]!.ShouldBeAssignableTo<IDictionary<string, (IEnumerable<ContentDocument> Posts, IEnumerable<ContentDocument> Pages)>>();
        taggedDocs.ShouldNotBeNull();
        taggedDocs!.Count.ShouldBe(3); // blazor, azure, asp-net

        // Verify azure tag has posts sorted by date descending (lorem-ipsum first, then hello-world)
        taggedDocs.ContainsKey("azure").ShouldBeTrue();
        var azurePosts = taggedDocs["azure"].Posts.ToList();
        azurePosts.Count.ShouldBe(2);
        azurePosts[0].Metadata.Slug.ShouldBe("blog/lorem-ipsum"); // 2026-01-03 - newer
        azurePosts[1].Metadata.Slug.ShouldBe("blog/hello-world"); // 2025-12-31 - older

        // Verify blazor tag has posts and pages
        taggedDocs.ContainsKey("blazor").ShouldBeTrue();
        var blazorPosts = taggedDocs["blazor"].Posts.ToList();
        var blazorPages = taggedDocs["blazor"].Pages.ToList();
        blazorPosts.Count.ShouldBe(1);
        blazorPages.Count.ShouldBe(1);
        blazorPosts[0].Metadata.Slug.ShouldBe("blog/hello-world");
        blazorPages[0].Metadata.Slug.ShouldBe("getting-started");
    }

    [Fact]
    public async Task Given_DocumentsWithMixedCaseTags_When_BuildAsync_Invoked_Then_It_Should_NormalizeTagsToLowercase()
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
            Metadata = new ContentMetadata
            {
                Title = "Post",
                Slug = "blog/post",
                Tags = ["Azure"]
            }
        };

        var page = new ContentDocument
        {
            Kind = ContentKind.Page,
            Markdown = "# Page",
            Metadata = new ContentMetadata
            {
                Title = "Page",
                Slug = "page",
                Tags = ["azure"]
            }
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

        var capturedTagViewParams = new List<IDictionary<string, object?>>();

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
        renderer
            .RenderAsync<TestTagListView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("TAGLIST"));
        renderer
            .RenderAsync<TestTagView>(Arg.Any<Type>(), Arg.Do<IDictionary<string, object?>>(p => capturedTagViewParams.Add(p)), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("TAG"));

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
        await generator.BuildAsync<TestMainLayout, TestIndexView, TestPostView, TestPageView, TestNotFoundView, TestTagListView, TestTagView>(destination, preview: false, CancellationToken.None);

        // Assert
        fileSystem.File.Exists(fileSystem.Path.Combine(destination, "tags", "azure", "index.html")).ShouldBeTrue();

        await pluginRunner
              .Received()
              .RunPostHtmlAsync(Arg.Any<string>(), Arg.Is<ContentDocument>(d => d.Metadata.Slug == "tags/azure"), Arg.Any<CancellationToken>());

        await pluginRunner
              .DidNotReceive()
              .RunPostHtmlAsync(Arg.Any<string>(), Arg.Is<ContentDocument>(d => d.Metadata.Slug == "tags/Azure"), Arg.Any<CancellationToken>());

        capturedTagViewParams.Count.ShouldBe(1);
        capturedTagViewParams[0].ContainsKey("Tag").ShouldBeTrue();
        capturedTagViewParams[0]["Tag"].ShouldBe("azure");
    }

    [Fact]
    public async Task Given_404DocumentWithTags_When_BuildAsync_Invoked_Then_It_Should_NotGenerateTagPageFor404()
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
            Markdown = "# 404",
            Metadata = new ContentMetadata
            {
                Title = "404",
                Slug = "404.html",
                Tags = ["Hidden"]
            }
        };

        var post = new ContentDocument
        {
            Kind = ContentKind.Post,
            Markdown = "# Post",
            Metadata = new ContentMetadata
            {
                Title = "Post",
                Slug = "blog/post",
                Tags = ["Visible"]
            }
        };

        var contentLoader = Substitute.For<IContentLoader>();
        contentLoader
            .LoadAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<ContentDocument>>(new[] { notFoundPage, post }));

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

        IDictionary<string, object?>? capturedTagListParams = null;

        var renderer = Substitute.For<IComponentRenderer>();
        renderer
            .RenderAsync<TestIndexView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("INDEX"));
        renderer
            .RenderAsync<TestPostView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("POST"));
        renderer
            .RenderAsync<TestNotFoundView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("NOTFOUND"));
        renderer
            .RenderAsync<TestTagListView>(Arg.Any<Type>(), Arg.Do<IDictionary<string, object?>>(p => capturedTagListParams = p), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("TAGLIST"));
        renderer
            .RenderAsync<TestTagView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("TAG"));

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
        await generator.BuildAsync<TestMainLayout, TestIndexView, TestPostView, TestPageView, TestNotFoundView, TestTagListView, TestTagView>(destination, preview: false, CancellationToken.None);

        // Assert
        fileSystem.File.Exists(fileSystem.Path.Combine(destination, "tags", "visible", "index.html")).ShouldBeTrue();
        fileSystem.File.Exists(fileSystem.Path.Combine(destination, "tags", "hidden", "index.html")).ShouldBeFalse();

        capturedTagListParams.ShouldNotBeNull();
        var taggedDocs = capturedTagListParams!["TaggedDocuments"]!.ShouldBeAssignableTo<IDictionary<string, (IEnumerable<ContentDocument> Posts, IEnumerable<ContentDocument> Pages)>>();
        taggedDocs.ContainsKey("hidden").ShouldBeFalse();
        taggedDocs.ContainsKey("visible").ShouldBeTrue();
    }

    [Fact]
    public async Task Given_MultipleTaggedPages_When_BuildAsync_Invoked_Then_TaggedPages_Should_BeSortedByTitleAscending()
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

        var pageZeta = new ContentDocument
        {
            Kind = ContentKind.Page,
            Markdown = "# Zeta",
            Metadata = new ContentMetadata
            {
                Title = "Zeta",
                Slug = "zeta",
                Tags = ["docs"]
            }
        };

        var pageAlpha = new ContentDocument
        {
            Kind = ContentKind.Page,
            Markdown = "# Alpha",
            Metadata = new ContentMetadata
            {
                Title = "Alpha",
                Slug = "alpha",
                Tags = ["docs"]
            }
        };

        var contentLoader = Substitute.For<IContentLoader>();
        contentLoader
            .LoadAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<ContentDocument>>(new[] { pageZeta, pageAlpha }));

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

        IDictionary<string, object?>? capturedTagListParams = null;

        var renderer = Substitute.For<IComponentRenderer>();
        renderer
            .RenderAsync<TestIndexView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("INDEX"));
        renderer
            .RenderAsync<TestPageView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("PAGE"));
        renderer
            .RenderAsync<TestNotFoundView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("NOTFOUND"));
        renderer
            .RenderAsync<TestTagListView>(Arg.Any<Type>(), Arg.Do<IDictionary<string, object?>>(p => capturedTagListParams = p), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("TAGLIST"));
        renderer
            .RenderAsync<TestTagView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("TAG"));

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
        await generator.BuildAsync<TestMainLayout, TestIndexView, TestPostView, TestPageView, TestNotFoundView, TestTagListView, TestTagView>(destination, preview: false, CancellationToken.None);

        // Assert
        capturedTagListParams.ShouldNotBeNull();
        var taggedDocs = capturedTagListParams!["TaggedDocuments"]!.ShouldBeAssignableTo<IDictionary<string, (IEnumerable<ContentDocument> Posts, IEnumerable<ContentDocument> Pages)>>();

        taggedDocs.ContainsKey("docs").ShouldBeTrue();
        var docsPages = taggedDocs["docs"].Pages.ToList();
        docsPages.Count.ShouldBe(2);
        docsPages[0].Metadata.Title.ShouldBe("Alpha");
        docsPages[1].Metadata.Title.ShouldBe("Zeta");
    }

    [Fact]
    public async Task Given_NoDocumentsWithTags_When_BuildAsync_Invoked_Then_It_Should_NotGenerateTagPages()
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
            Markdown = "# Post without tags",
            Metadata = new ContentMetadata
            {
                Title = "No Tags Post",
                Slug = "blog/no-tags",
                Tags = [] // Empty tags
            }
        };

        var contentLoader = Substitute.For<IContentLoader>();
        contentLoader
            .LoadAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<ContentDocument>>(new[] { post }));

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
            .RenderAsync<TestPostView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("POST"));
        renderer
            .RenderAsync<TestNotFoundView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("NOTFOUND"));
        renderer
            .RenderAsync<TestTagListView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("TAGLIST"));
        renderer
            .RenderAsync<TestTagView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("TAG"));

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
        await generator.BuildAsync<TestMainLayout, TestIndexView, TestPostView, TestPageView, TestNotFoundView, TestTagListView, TestTagView>(destination, preview: false, CancellationToken.None);

        // Assert - no tag pages should be generated
        fileSystem.Directory.Exists(fileSystem.Path.Combine(destination, "tags")).ShouldBeFalse();

        // Verify tag views were never rendered
        await renderer.DidNotReceive().RenderAsync<TestTagListView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>());
        await renderer.DidNotReceive().RenderAsync<TestTagView>(Arg.Any<Type>(), Arg.Any<IDictionary<string, object?>>(), Arg.Any<CancellationToken>());
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

internal sealed class TestTagListView : ScissorHands.Theme.TagListViewBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
    }
}

internal sealed class TestTagView : ScissorHands.Theme.TagViewBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
    }
}
