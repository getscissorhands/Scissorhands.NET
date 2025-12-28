using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Web.Loaders;
using ScissorHands.Web.Tests.TestDoubles;

using System.IO.Abstractions.TestingHelpers;

namespace ScissorHands.Web.Tests.Loaders;

public class ContentLoaderTests
{
    [Fact]
    public async Task Given_MarkdownWithoutFrontMatter_When_LoadAsync_Invoked_Then_It_Should_DefaultTitle_And_InferSlug()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var root = fileSystem.Path.GetPathRoot(Environment.CurrentDirectory) ?? fileSystem.Path.DirectorySeparatorChar.ToString();
        var baseRoot = fileSystem.Path.Combine(root, "base");
        var contentsRoot = fileSystem.Path.Combine(baseRoot, "contents");
        var themesRoot = fileSystem.Path.Combine(baseRoot, "themes");

        var postsRoot = fileSystem.Path.Combine(contentsRoot, "posts");
        var pagesRoot = fileSystem.Path.Combine(contentsRoot, "pages");

        fileSystem.AddDirectory(postsRoot);
        fileSystem.AddDirectory(pagesRoot);

        var postPath = fileSystem.Path.Combine(postsRoot, "hello-world.md");
        fileSystem.AddFile(postPath, new MockFileData("# Hello\n\nBody"));

        var options = new SiteManifest { IncludeDateInPostUrl = false };
        var paths = new TestAppPaths(basePath: baseRoot, contentsRoot, themesRoot);
        var logger = Substitute.For<ILogger<ContentLoader>>();

        var loader = new ContentLoader(paths, fileSystem, options, logger);

        // Act
        var docs = (await loader.LoadAsync(CancellationToken.None)).ToList();

        // Assert
        docs.Count.ShouldBe(1);
        var doc = docs[0];
        doc.Kind.ShouldBe(ContentKind.Post);
        doc.SourcePath.ShouldBe(postPath);
        doc.Markdown.ShouldBe("# Hello\n\nBody");
        doc.Metadata.Title.ShouldBe("hello-world");
        doc.Metadata.Slug.ShouldBe("hello-world");
    }

    [Fact]
    public async Task Given_FrontMatterWithCustomSlugAndPublished_When_LoadAsync_Invoked_Then_It_Should_ApplyDatePrefix_When_Enabled()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var root = fileSystem.Path.GetPathRoot(Environment.CurrentDirectory) ?? fileSystem.Path.DirectorySeparatorChar.ToString();
        var baseRoot = fileSystem.Path.Combine(root, "base");
        var contentsRoot = fileSystem.Path.Combine(baseRoot, "contents");
        var themesRoot = fileSystem.Path.Combine(baseRoot, "themes");

        var postsRoot = fileSystem.Path.Combine(contentsRoot, "posts");
        var pagesRoot = fileSystem.Path.Combine(contentsRoot, "pages");

        fileSystem.AddDirectory(postsRoot);
        fileSystem.AddDirectory(pagesRoot);

        var postPath = fileSystem.Path.Combine(postsRoot, "post.md");
        var markdown = string.Join("\n", new[]
        {
            "---",
            "title: Hello",
            "slug: /my-post/",
            "published: 2024-01-02",
            "tags:",
            "  - one",
            "  - two",
            "---",
            "# Content",
        });

        fileSystem.AddFile(postPath, new MockFileData(markdown));

        var options = new SiteManifest { IncludeDateInPostUrl = true };
        var paths = new TestAppPaths(basePath: baseRoot, contentsRoot, themesRoot);
        var logger = Substitute.For<ILogger<ContentLoader>>();

        var loader = new ContentLoader(paths, fileSystem, options, logger);

        // Act
        var docs = (await loader.LoadAsync(CancellationToken.None)).ToList();

        // Assert
        docs.Count.ShouldBe(1);
        var doc = docs[0];
        doc.Kind.ShouldBe(ContentKind.Post);
        doc.Metadata.Title.ShouldBe("Hello");
        doc.Metadata.Tags.ShouldBe(new[] { "one", "two" });
        doc.Markdown.ShouldBe("# Content");
        doc.Metadata.Slug.ShouldBe("2024/01/02/my-post");
    }

    [Fact]
    public async Task Given_DraftFrontMatter_When_LoadAsync_Invoked_Then_It_Should_SkipDraftDocuments()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var root = fileSystem.Path.GetPathRoot(Environment.CurrentDirectory) ?? fileSystem.Path.DirectorySeparatorChar.ToString();
        var baseRoot = fileSystem.Path.Combine(root, "base");
        var contentsRoot = fileSystem.Path.Combine(baseRoot, "contents");
        var themesRoot = fileSystem.Path.Combine(baseRoot, "themes");

        var postsRoot = fileSystem.Path.Combine(contentsRoot, "posts");
        var pagesRoot = fileSystem.Path.Combine(contentsRoot, "pages");

        fileSystem.AddDirectory(postsRoot);
        fileSystem.AddDirectory(pagesRoot);

        var postPath = fileSystem.Path.Combine(postsRoot, "draft.md");
        var markdown = string.Join("\n", new[]
        {
            "---",
            "title: Draft",
            "draft: true",
            "---",
            "# Content",
        });

        fileSystem.AddFile(postPath, new MockFileData(markdown));

        var options = new SiteManifest();
        var paths = new TestAppPaths(basePath: baseRoot, contentsRoot, themesRoot);
        var logger = Substitute.For<ILogger<ContentLoader>>();

        var loader = new ContentLoader(paths, fileSystem, options, logger);

        // Act
        var docs = (await loader.LoadAsync(CancellationToken.None)).ToList();

        // Assert
        docs.ShouldBeEmpty();
    }
}
