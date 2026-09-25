using System.IO.Abstractions.TestingHelpers;

using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Web.Loaders;
using ScissorHands.Web.Tests.TestDoubles;

namespace ScissorHands.Web.Tests.Loaders;

public class ContentLoaderDirectoryIndexTests
{
    [Theory]
    [InlineData("pages", "parent/index.md", "parent")]
    [InlineData("pages", "parent/group/index.md", "parent/group")]
    [InlineData("pages", "parent/INDEX.md", "parent")]
    [InlineData("pages", "index.md", "index")]
    [InlineData("pages", "parent/index/index.md", "parent/index")]
    [InlineData("pages", "index/child.md", "index/child")]
    [InlineData("pages", "parent/index-other.md", "parent/index-other")]
    [InlineData("posts", "parent/index.md", "parent/index")]
    [InlineData("posts", "index.md", "index")]
    public async Task Given_ContentPathWithoutFrontmatter_When_Loaded_Then_It_Should_CollapseOnlyNestedPageIndexes(
        string directory,
        string relativePath,
        string expectedSlug)
    {
        var (loader, sourcePath) = CreateLoader(directory, relativePath, null);

        var document = (await loader.LoadAsync(Xunit.TestContext.Current.CancellationToken)).ShouldHaveSingleItem();

        document.SourcePath.ShouldBe(sourcePath);
        document.Metadata.Slug.ShouldBe(expectedSlug);
        document.Metadata.Title.ShouldBe(Path.GetFileNameWithoutExtension(sourcePath));
        document.Metadata.ShowInNavigation.ShouldBeFalse();
        document.Markdown.ShouldBe("# Content");
    }

    [Theory]
    [InlineData("", "parent")]
    [InlineData("slug: ''", "parent")]
    [InlineData("slug: '   '", "parent")]
    [InlineData("slug: null", "parent")]
    [InlineData("slug: parent/index", "parent/index")]
    [InlineData("slug: /custom/landing/", "custom/landing")]
    [InlineData("slug: /", "")]
    public async Task Given_PageIndexFrontmatter_When_Loaded_Then_It_Should_PreserveExplicitSlugOverrides(
        string slugField,
        string expectedSlug)
    {
        var (loader, _) = CreateLoader("pages", "parent/index.md", $"title: Parent\nshow_in_navigation: true\n{slugField}");

        var document = (await loader.LoadAsync(Xunit.TestContext.Current.CancellationToken)).ShouldHaveSingleItem();

        document.Metadata.Slug.ShouldBe(expectedSlug);
        document.Metadata.Title.ShouldBe("Parent");
        document.Metadata.ShowInNavigation.ShouldBeTrue();
    }

    [Theory]
    [InlineData("pages", "parent/index.md", "", "parent")]
    [InlineData("pages", "index.md", "", "index")]
    [InlineData("pages", "parent/index.md", "slug: custom", "custom")]
    [InlineData("pages", "parent/index.md", "slug: 404.html", "404.html")]
    [InlineData("posts", "parent/index.md", "", "2026/09/13/parent/index")]
    [InlineData("posts", "hello.md", "slug: custom", "2026/09/13/custom")]
    [InlineData("pages", "it/hello.md", "", "it/hello")]
    public async Task Given_LocaleAndDateOptions_When_IndexLoaded_Then_It_Should_PreserveUrlConventions(
        string directory,
        string relativePath,
        string slugField,
        string expectedSlug)
    {
        var site = new SiteManifest { BaseUrl = "/site/", Locales = ["ko_KR"], UseDateInPostUrl = true };
        var (loader, _) = CreateLoader(directory, relativePath, $"published: 2026-09-13\n{slugField}", site);

        var document = (await loader.LoadAsync(Xunit.TestContext.Current.CancellationToken)).ShouldHaveSingleItem();

        document.Metadata.Slug.ShouldBe(expectedSlug);
        document.Metadata.Locale.ShouldBe("ko-kr");
    }

    [Fact]
    public async Task Given_LocaleFolderWithoutLocaleMetadata_When_Loaded_Then_It_Should_UseSiteLocaleWithoutStrippingTheFolder()
    {
        var site = new SiteManifest { Locales = ["en-US"] };
        var (loader, _) = CreateLoader("pages", "ko-kr/about.md", null, site);

        var document = (await loader.LoadAsync(Xunit.TestContext.Current.CancellationToken)).ShouldHaveSingleItem();

        document.Metadata.Locale.ShouldBe("en-us");
        document.Metadata.Slug.ShouldBe("ko-kr/about");
    }

    private static (ContentLoader Loader, string SourcePath) CreateLoader(
        string directory,
        string relativePath,
        string? frontmatter,
        SiteManifest? site = null)
    {
        var fileSystem = new MockFileSystem();
        var root = fileSystem.Path.GetPathRoot(Environment.CurrentDirectory) ?? fileSystem.Path.DirectorySeparatorChar.ToString();
        var baseRoot = fileSystem.Path.Combine(root, "base");
        var contentsRoot = fileSystem.Path.Combine(baseRoot, "contents");
        var themesRoot = fileSystem.Path.Combine(baseRoot, "themes");
        var sourcePath = fileSystem.Path.Combine(contentsRoot, directory, relativePath.Replace('/', fileSystem.Path.DirectorySeparatorChar));
        var markdown = frontmatter is null ? "# Content" : $"---\n{frontmatter}\n---\n# Content";
        fileSystem.AddFile(sourcePath, new MockFileData(markdown));
        var loader = new ContentLoader(
            new TestAppPaths(baseRoot, contentsRoot, themesRoot),
            fileSystem,
            site ?? new SiteManifest { UseDateInPostUrl = false },
            Substitute.For<ILogger<ContentLoader>>());

        return (loader, sourcePath);
    }
}
