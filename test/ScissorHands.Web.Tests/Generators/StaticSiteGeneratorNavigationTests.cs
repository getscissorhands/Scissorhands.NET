using System.IO.Abstractions.TestingHelpers;

using AngleSharp.Html.Parser;

using Microsoft.AspNetCore.Components;
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
    [InlineData(false, "/", false, false)]
    [InlineData(true, "/", true, false)]
    [InlineData(false, "/docs/", true, false)]
    [InlineData(true, "/docs/", false, false)]
    [InlineData(false, "/", false, true)]
    [InlineData(true, "/", true, true)]
    [InlineData(false, "/docs/", true, true)]
    [InlineData(true, "/docs/", false, true)]
    public async Task Given_OptedInPages_When_BuildInvoked_Then_It_Should_RenderNavigationOnEverySurface(
        bool preview,
        string baseUrl,
        bool customNotFound,
        bool missingDeployment)
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
        AddContent("pages", "docs.md", "title: Docs\nshow_in_navigation: true");
        AddContent("pages", "quickstart.md", "title: Quickstart\nslug: docs/quickstart\nshow_in_navigation: true");
        if (!missingDeployment)
        {
            AddContent("pages", "deployment.md", "title: Deployment\nslug: docs/deployment\nshow_in_navigation: true");
        }
        AddContent("pages", "github-pages.md", "title: GitHub Pages\nslug: docs/deployment/github-pages\nshow_in_navigation: true");
        AddContent("pages", "netlify.md", "title: Netlify\nslug: docs/deployment/netlify\nshow_in_navigation: false");
        AddContent("pages", "docs-other.md", "title: Side docs\nshow_in_navigation: true");
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
        var renderer = new RecordingRenderer(new ComponentRenderer(
            provider.GetRequiredService<IServiceScopeFactory>(),
            provider.GetRequiredService<ILoggerFactory>()));
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

        var expectedPaths = new List<string>
        {
            "index.html",
            "404.html",
            "en-us/guides/about & team/index.html",
            "en-us/zebra/index.html",
            "en-us/docs/index.html",
            "en-us/docs/quickstart/index.html",
            "en-us/docs/deployment/github-pages/index.html",
            "en-us/docs/deployment/netlify/index.html",
            "en-us/docs-other/index.html",
            "en-us/default/index.html",
            "en-us/hidden/index.html",
            "en-us/post/index.html",
            "tags/index.html",
            "tags/sample/index.html",
        };
        var deploymentPath = fileSystem.Path.Combine(destination, "en-us", "docs", "deployment", "index.html");
        fileSystem.File.Exists(deploymentPath).ShouldBe(!missingDeployment);
        if (!missingDeployment)
        {
            expectedPaths.Add("en-us/docs/deployment/index.html");
        }
        renderer.Navigations.Count.ShouldBe(expectedPaths.Count);
        renderer.Navigations[0].Pages.Count.ShouldBe(missingDeployment ? 6 : 7);

        var parser = new HtmlParser();
        foreach (var path in expectedPaths)
        {
            var outputPath = fileSystem.Path.Combine(destination, path.Replace('/', fileSystem.Path.DirectorySeparatorChar));
            fileSystem.File.Exists(outputPath).ShouldBeTrue(path);
            using var html = parser.ParseDocument(fileSystem.File.ReadAllText(outputPath));
            var links = html.QuerySelectorAll(".site-header nav a");
            var expectedTitles = new List<string> { "Home", aboutTitle, "Side docs", "Docs", "GitHub Pages", "Quickstart", "Zebra", "Tags" };
            var expectedUrls = new List<string>
            {
                ".", "en-us/guides/about%20%26%20team", "en-us/docs-other", "en-us/docs",
                "en-us/docs/deployment/github-pages", "en-us/docs/quickstart", "en-us/zebra", "tags"
            };
            if (!missingDeployment)
            {
                expectedTitles = ["Home", aboutTitle, "Docs", "Deployment", "GitHub Pages", "Quickstart", "Side docs", "Zebra", "Tags"];
                expectedUrls = [
                    ".", "en-us/guides/about%20%26%20team", "en-us/docs", "en-us/docs/deployment",
                    "en-us/docs/deployment/github-pages", "en-us/docs/quickstart", "en-us/docs-other", "en-us/zebra", "tags",
                ];
            }
            links.Select(link => link.TextContent).ShouldBe(expectedTitles);
            links.Select(link => link.GetAttribute("href")).ShouldBe(expectedUrls);
            var docsItem = html.QuerySelector("nav a[href='en-us/docs']")!.Closest("li")!;
            docsItem.QuerySelectorAll(":scope > ul > li > .navigation-link")
                .Select(line => line.QuerySelector("a, .navigation-label")!.TextContent).ShouldBe(["Deployment", "Quickstart"]);
            var deploymentLabel = missingDeployment
                ? html.QuerySelectorAll("nav .navigation-label").Single(label => label.TextContent == "Deployment")
                : html.QuerySelector("nav a[href='en-us/docs/deployment']")!;
            var deploymentItem = deploymentLabel.Closest("li")!;
            (deploymentItem.QuerySelector(":scope > .navigation-link > a") is null).ShouldBe(missingDeployment);
            deploymentItem.QuerySelectorAll(":scope > ul > li > .navigation-link > a")
                .Select(link => link.TextContent).ShouldBe(["GitHub Pages"]);
            html.QuerySelectorAll("nav .navigation-label").Select(label => label.TextContent)
                .ShouldBe(missingDeployment ? ["Guides", "Deployment"] : ["Guides"]);
            html.QuerySelectorAll("nav script").ShouldBeEmpty();
            html.QuerySelector("base")!.GetAttribute("href").ShouldBe(baseUrl);
            var siteBase = new Uri($"https://example.com{baseUrl}");
            new Uri(siteBase, links.Single(link => link.TextContent == aboutTitle).GetAttribute("href")!).AbsoluteUri.ShouldBe(
                $"https://example.com{baseUrl}en-us/guides/about%20%26%20team");
        }

        fileSystem.File.Exists(fileSystem.Path.Combine(destination, "en-us", "draft", "index.html")).ShouldBeFalse();
        using var index = parser.ParseDocument(fileSystem.File.ReadAllText(fileSystem.Path.Combine(destination, "index.html")));
        index.QuerySelectorAll(".post-list .post-link").Select(link => link.TextContent).ShouldBe(["Post"]);
        site.IsPreview.ShouldBe(preview);

        if (missingDeployment)
        {
            AddContent("pages", "github-pages.md", "title: GitHub Pages\nslug: docs/deployment/github-pages\nshow_in_navigation: false");

            await BuildAsync();
            AssertNavigationTitles(["Home", aboutTitle, "Side docs", "Docs", "Quickstart", "Zebra", "Tags"]);
            foreach (var path in expectedPaths)
            {
                var outputPath = fileSystem.Path.Combine(destination, path.Replace('/', fileSystem.Path.DirectorySeparatorChar));
                using var html = parser.ParseDocument(fileSystem.File.ReadAllText(outputPath));
                html.QuerySelectorAll("nav .navigation-label").Select(label => label.TextContent).ShouldBe(["Guides"]);
                html.QuerySelectorAll(".navigation-toggle[aria-label='Toggle Deployment pages']").ShouldBeEmpty();
            }
            fileSystem.File.Exists(deploymentPath).ShouldBeFalse();
            AddContent("pages", "github-pages.md", "title: GitHub Pages\nslug: docs/deployment/github-pages\nshow_in_navigation: true");
        }

        AddContent("pages", "zebra.md", "title: Zebra\nshow_in_navigation: false");
        AddContent("pages", "about.md", $"title: '{aboutTitle}'\nslug: guides/about & team");
        AddContent("pages", "deployment.md", "title: Deployment\nslug: docs/deployment\nshow_in_navigation: false");

        await BuildAsync();
        AssertNavigationTitles(["Home", "Side docs", "Docs", "Quickstart", "Tags"]);

        AddContent("pages", "docs.md", "title: Docs");
        if (missingDeployment)
        {
            fileSystem.File.Delete(fileSystem.Path.Combine(contentsRoot, "pages", "deployment.md"));
        }
        else
        {
            AddContent("pages", "deployment.md", "title: Deployment\nslug: docs/deployment\nshow_in_navigation: true");
        }

        await BuildAsync();
        AssertNavigationTitles(["Home", "Side docs", "Tags"]);

        AddContent("pages", "docs-other.md", "title: Side docs\nshow_in_navigation: false");

        await BuildAsync();
        AssertNavigationTitles(["Home", "Tags"]);

        void AssertNavigationTitles(string[] expected)
        {
            foreach (var path in expectedPaths)
            {
                var outputPath = fileSystem.Path.Combine(destination, path.Replace('/', fileSystem.Path.DirectorySeparatorChar));
                using var html = parser.ParseDocument(fileSystem.File.ReadAllText(outputPath));
                html.QuerySelectorAll(".site-header nav a").Select(link => link.TextContent).ShouldBe(expected);
                if (expected.Length == 2)
                {
                    html.QuerySelectorAll(".navigation-item").ShouldBeEmpty();
                }
            }
        }

        void AddContent(string directory, string name, string frontMatter)
        {
            var path = fileSystem.Path.Combine(contentsRoot, directory, name);
            fileSystem.AddFile(path, new MockFileData($"---\n{frontMatter}\n---\n# Body"));
        }

        async Task BuildAsync()
        {
            var before = renderer.Navigations.Count;
            await generator.BuildAsync<MainLayout, IndexView, PostView, PageView, NotFoundView, TagListView, TagView>(
                destination, preview, Xunit.TestContext.Current.CancellationToken);

            var navigation = renderer.Navigations[before];
            foreach (var rendered in renderer.Navigations.Skip(before))
            {
                rendered.Tree.ShouldBeSameAs(navigation.Tree);
                rendered.Pages.ShouldBeSameAs(navigation.Pages);
            }
            if (before > 0)
            {
                navigation.Tree.ShouldNotBeSameAs(renderer.Navigations[before - 1].Tree);
            }
        }
    }

    private sealed class RecordingRenderer(IComponentRenderer inner) : IComponentRenderer
    {
        public List<(IReadOnlyList<ContentDocument> Pages, IReadOnlyList<NavigationNode> Tree)> Navigations { get; } = [];

        public Task<string> RenderAsync<TComponent>(Type layoutType, IDictionary<string, object?> parameters, CancellationToken cancellationToken = default)
            where TComponent : IComponent
        {
            var pages = parameters["NavigationPages"].ShouldBeAssignableTo<IReadOnlyList<ContentDocument>>();
            var tree = parameters["NavigationTree"].ShouldBeAssignableTo<IReadOnlyList<NavigationNode>>();
            pages.ShouldNotBeNull();
            tree.ShouldNotBeNull();
            Navigations.Add((pages, tree));
            return inner.RenderAsync<TComponent>(layoutType, parameters, cancellationToken);
        }
    }
}
