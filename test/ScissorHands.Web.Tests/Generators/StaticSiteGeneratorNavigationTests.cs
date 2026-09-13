using System.IO.Abstractions.TestingHelpers;

using AngleSharp.Html.Parser;

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

public class StaticSiteGeneratorNavigationTests
{
    [Theory]
    [InlineData(false, "/", false)]
    [InlineData(true, "/", true)]
    [InlineData(false, "/docs/", true)]
    [InlineData(true, "/docs/", false)]
    public async Task Given_OptedInPages_When_BuildInvoked_Then_It_Should_RenderNavigationOnEverySurface(
        bool preview,
        string baseUrl,
        bool customNotFound)
    {
        var fileSystem = new MockFileSystem();
        var root = fileSystem.Path.GetPathRoot(Environment.CurrentDirectory) ?? fileSystem.Path.DirectorySeparatorChar.ToString();
        var baseRoot = fileSystem.Path.Combine(root, "base");
        var contentsRoot = fileSystem.Path.Combine(baseRoot, "contents");
        var themesRoot = fileSystem.Path.Combine(baseRoot, "themes");
        var destination = fileSystem.Path.Combine(baseRoot, preview ? "preview" : "dist");
        var paths = new TestAppPaths(baseRoot, contentsRoot, themesRoot);
        var site = new SiteManifest { BaseUrl = baseUrl, UseLocaleInUrl = true, UseDateInPostUrl = false };
        const string aboutTitle = "About <script>alert(1)</script> & team";
        AddContent("pages", "zebra.md", "title: Zebra\nshow_in_navigation: true");
        AddContent("pages", "about.md", $"title: '{aboutTitle}'\nslug: guides/about & team\nshow_in_navigation: true");
        AddContent("pages", "default.md", "title: Default");
        AddContent("pages", "hidden.md", "title: Hidden\nshow_in_navigation: false");
        AddContent("pages", "draft.md", "title: Draft\ndraft: true\nshow_in_navigation: true");
        AddContent("posts", "post.md", "title: Post\nshow_in_navigation: true\ntags: [sample]");
        if (customNotFound)
        {
            AddContent("pages", "not-found.md", "title: Missing\nslug: 404.html\nshow_in_navigation: true");
        }

        var themeService = Substitute.For<IThemeService>();
        themeService.LoadManifestAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new ThemeManifest { Slug = "default" });
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(themeService);
        using var provider = services.BuildServiceProvider();
        var renderer = new ComponentRenderer(
            provider.GetRequiredService<IServiceScopeFactory>(),
            provider.GetRequiredService<ILoggerFactory>());
        var markdownService = Substitute.For<IMarkdownService>();
        markdownService.ToHtmlAsync(Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>()).Returns("<p>Body</p>");
        var pluginRunner = Substitute.For<IPluginRunner>();
        pluginRunner.Manifests.Returns([]);
        pluginRunner.RunPreMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>())
            .Returns(call => call.ArgAt<ContentDocument>(0));
        pluginRunner.RunPostMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>())
            .Returns(call => call.ArgAt<ContentDocument>(0));
        pluginRunner.RunPostHtmlAsync(Arg.Any<string>(), Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>())
            .Returns(call => call.ArgAt<string>(0));
        var generator = new StaticSiteGenerator(
            new ContentLoader(paths, fileSystem, site, Substitute.For<ILogger<ContentLoader>>()),
            markdownService,
            pluginRunner,
            themeService,
            renderer,
            paths,
            fileSystem,
            site,
            Substitute.For<ILogger<StaticSiteGenerator>>());

        await BuildAsync();

        var expectedPaths = new[]
        {
            "index.html",
            "404.html",
            "en-us/guides/about & team/index.html",
            "en-us/zebra/index.html",
            "en-us/default/index.html",
            "en-us/hidden/index.html",
            "en-us/post/index.html",
            "tags/index.html",
            "tags/sample/index.html",
        };
        var parser = new HtmlParser();
        foreach (var path in expectedPaths)
        {
            var outputPath = fileSystem.Path.Combine(destination, path.Replace('/', fileSystem.Path.DirectorySeparatorChar));
            fileSystem.File.Exists(outputPath).ShouldBeTrue(path);
            using var html = parser.ParseDocument(fileSystem.File.ReadAllText(outputPath));
            var links = html.QuerySelectorAll("nav a");
            links.Select(link => link.TextContent).ShouldBe(["Home", "Tags", aboutTitle, "Zebra"]);
            links.Select(link => link.GetAttribute("href")).ShouldBe([".", "tags", "en-us/guides/about%20%26%20team", "en-us/zebra"]);
            html.QuerySelectorAll("nav script").ShouldBeEmpty();
            html.QuerySelector("base")!.GetAttribute("href").ShouldBe(baseUrl);
            var siteBase = new Uri($"https://example.com{baseUrl}");
            new Uri(siteBase, links[2].GetAttribute("href")!).AbsoluteUri.ShouldBe(
                $"https://example.com{baseUrl}en-us/guides/about%20%26%20team");
        }

        fileSystem.File.Exists(fileSystem.Path.Combine(destination, "en-us", "draft", "index.html")).ShouldBeFalse();
        using var index = parser.ParseDocument(fileSystem.File.ReadAllText(fileSystem.Path.Combine(destination, "index.html")));
        index.QuerySelectorAll(".post-list .post-link").Select(link => link.TextContent).ShouldBe(["Post"]);
        site.IsPreview.ShouldBe(preview);

        AddContent("pages", "zebra.md", "title: Zebra\nshow_in_navigation: false");
        AddContent("pages", "about.md", $"title: '{aboutTitle}'\nslug: guides/about & team");

        await BuildAsync();

        foreach (var path in expectedPaths)
        {
            var outputPath = fileSystem.Path.Combine(destination, path.Replace('/', fileSystem.Path.DirectorySeparatorChar));
            using var html = parser.ParseDocument(fileSystem.File.ReadAllText(outputPath));
            html.QuerySelectorAll("nav a").Select(link => link.TextContent).ShouldBe(["Home", "Tags"]);
        }

        void AddContent(string directory, string name, string frontMatter)
        {
            var path = fileSystem.Path.Combine(contentsRoot, directory, name);
            fileSystem.AddFile(path, new MockFileData($"---\n{frontMatter}\n---\n# Body"));
        }

        Task BuildAsync() => generator.BuildAsync<MainLayout, IndexView, PostView, PageView, NotFoundView, TagListView, TagView>(
            destination, preview, Xunit.TestContext.Current.CancellationToken);
    }
}
