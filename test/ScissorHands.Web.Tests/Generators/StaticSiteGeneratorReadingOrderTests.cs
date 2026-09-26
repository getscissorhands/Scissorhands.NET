using System.IO.Abstractions.TestingHelpers;

using Microsoft.AspNetCore.Components;
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

public class StaticSiteGeneratorReadingOrderTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Given_MixedPages_When_Generated_Then_It_Should_ShareOneCombinedSequenceAndAdjacentLinks(bool preview)
    {
        var parent = Page("Z Parent", "docs", @"parent\index.md");
        var child = Page("Y Child", "docs/zulu", @"parent\01-child.md");
        var grandchild = Page("X Grandchild", "docs/group/grand child", @"parent\02-group\visible-grandchild.md");
        var child2 = Page("W Child 2", "alpha", @"parent\03-child-2.md");
        var sourceLessA = Page("A Source-less", "loose/a");
        var sourceLessZ = Page("Z Source-less", "loose/z");
        var hidden = Page("Hidden", "hidden", "00-hidden.md", visible: false);
        var suppressed = Page("Suppressed", "hidden/child", "00-child.md");
        var draft = new ContentDocument { Kind = ContentKind.Page, Metadata = new() { Title = "Draft", Slug = "draft", Draft = true, ShowInNavigation = true } };
        var post = new ContentDocument { Kind = ContentKind.Post, Metadata = new() { Title = "Post", Slug = "post", ShowInNavigation = true } };
        var notFound = Page("Not found", "404.html", visible: true);
        var expected = new[] { parent, child, grandchild, child2, sourceLessA, sourceLessZ };
        var documents = new List<ContentDocument> { sourceLessZ, child2, suppressed, child, sourceLessA, hidden, grandchild, parent, post, notFound };
        if (!preview)
        {
            documents.Add(draft);
        }
        var fixture = new Fixture(documents);

        await fixture.BuildAsync(preview);

        foreach (var call in fixture.Renderer.Calls)
        {
            call.Pages.ShouldBe(expected);
            call.Pages.ShouldBeSameAs(fixture.Renderer.Calls[0].Pages);
            call.Tree.ShouldBeSameAs(fixture.Renderer.Calls[0].Tree);
        }

        for (var index = 0; index < expected.Length; index++)
        {
            var navigation = fixture.Renderer.ForDocument(expected[index]).Navigation;
            (navigation.Previous?.Title).ShouldBe(index == 0 ? null : expected[index - 1].Metadata.Title);
            (navigation.Next?.Title).ShouldBe(index + 1 == expected.Length ? null : expected[index + 1].Metadata.Title);
        }

        fixture.Renderer.ForDocument(child).Navigation.Next!.Url.ShouldBe("docs/group/grand%20child");
        fixture.Renderer.ForDocument(grandchild).Navigation.Next!.Url.ShouldBe("alpha");
        fixture.Renderer.ForDocument(child2).Navigation.Next!.Url.ShouldBe("loose/a");
        fixture.Renderer.ForDocument(sourceLessA).Navigation.Previous!.Url.ShouldBe("alpha");
        foreach (var call in fixture.Renderer.Calls.Where(call => call.Document is null || !expected.Contains(call.Document)))
        {
            call.Navigation.Previous.ShouldBeNull();
            call.Navigation.Next.ShouldBeNull();
        }

        var taggedPages = fixture.Renderer.Calls.Single(call => call.View == typeof(TagView)).Parameters["TaggedPages"];
        taggedPages.ShouldNotBeNull();
        var tagPages = taggedPages.ShouldBeAssignableTo<IEnumerable<ContentDocument>>();
        tagPages.Select(page => page.Metadata.Title).ShouldBe(expected.Select(page => page.Metadata.Title).Order());
        fixture.Site.IsPreview.ShouldBe(preview);
    }

    [Fact]
    public async Task Given_OnlySourceLessPages_When_Generated_Then_It_Should_UseTitleAndSlugWithoutIdentityCollisions()
    {
        var a = Page("Same", "a");
        var b = Page("Same", "b");
        var z = Page("Zebra", "z");
        var fixture = new Fixture([z, b, a]);

        await fixture.BuildAsync();

        fixture.Renderer.Calls[0].Pages.ShouldBe([a, b, z]);
        fixture.Renderer.ForDocument(a).Navigation.Next!.Url.ShouldBe("b");
        fixture.Renderer.ForDocument(b).Navigation.Previous!.Url.ShouldBe("a");
        fixture.Renderer.ForDocument(b).Navigation.Next!.Url.ShouldBe("z");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task Given_EmptyOrSingleSequence_When_Generated_Then_It_Should_HaveNoAdjacentLinks(int count)
    {
        var fixture = new Fixture(count == 0 ? [] : [Page("Only", "only", "index.md")]);

        await fixture.BuildAsync();

        foreach (var call in fixture.Renderer.Calls)
        {
            call.Navigation.Previous.ShouldBeNull();
            call.Navigation.Next.ShouldBeNull();
        }
    }

    [Fact]
    public async Task Given_PluginReplacement_When_Generated_Then_It_Should_KeepOriginalIdentityAndNavigationSnapshot()
    {
        var first = Page("First", "first", "01-first.md");
        var second = Page("Second", "second", "02-second.md");
        var replacement = new ContentDocument
        {
            Kind = ContentKind.Page,
            Metadata = first.Metadata with { Title = "Changed", Slug = "changed", ShowInNavigation = false },
        };
        var fixture = new Fixture([second, first]);
        fixture.Preprocess = page => ReferenceEquals(page, first) ? replacement : page;

        await fixture.BuildAsync();

        fixture.Renderer.ForDocument(replacement).Navigation.Next!.Url.ShouldBe("second");
        fixture.Renderer.ForDocument(second).Navigation.Previous!.Title.ShouldBe("First");
        fixture.Renderer.ForDocument(second).Navigation.Previous!.Url.ShouldBe("first");
        fixture.Renderer.Calls[0].Tree.Select(node => node.Title).ShouldBe(["First", "Second"]);
        await fixture.Plugins.Received(1).RunPreMarkdownAsync(first, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ChangedSourcesAndVisibility_When_Regenerated_Then_It_Should_RebuildWithoutMutatingPreviousSnapshots()
    {
        var first = Page("First", "first", "01-first.md");
        var second = Page("Second", "second", "02-second.md");
        var loose = Page("Loose", "loose");
        var fixture = new Fixture([first, second, loose]);
        await fixture.BuildAsync(preview: true);
        var original = fixture.Renderer.ForDocument(first).Navigation;
        var originalPages = fixture.Renderer.Calls[0].Pages;
        var renamed = Page("First", "first", "03-first.md");
        fixture.Documents = [Page("Loose", "loose", visible: false), renamed, second];
        fixture.Renderer.Calls.Clear();

        await fixture.BuildAsync(preview: true);

        fixture.Renderer.Calls[0].Pages.ShouldBe([second, renamed]);
        fixture.Renderer.Calls[0].Pages.ShouldNotBeSameAs(originalPages);
        fixture.Renderer.ForDocument(second).Navigation.Previous.ShouldBeNull();
        fixture.Renderer.ForDocument(second).Navigation.Next!.Url.ShouldBe("first");
        fixture.Renderer.ForDocument(renamed).Navigation.Next.ShouldBeNull();
        original.Next!.Url.ShouldBe("second");
        originalPages.ShouldBe([first, second, loose]);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("../outside.md")]
    [InlineData(@"parent\..\outside.md")]
    [InlineData("bad\0name.md")]
    [InlineData("directory/")]
    public async Task Given_InvalidSource_When_Generated_Then_It_Should_FailBeforeRenderingInsteadOfFallingBack(string source)
    {
        var fixture = new Fixture([Page("Invalid", "valid-route", source)]);

        var error = await Should.ThrowAsync<InvalidDataException>(() => fixture.BuildAsync());

        error.Message.ShouldContain("Page source path");
        fixture.Renderer.Calls.ShouldBeEmpty();
    }

    [Fact]
    public async Task Given_AbsoluteSourceOutsidePages_When_Generated_Then_It_Should_RejectTheSource()
    {
        var fixture = new Fixture([]);
        var outside = Path.Combine(Path.GetPathRoot(Environment.CurrentDirectory)!, "outside.md");
        fixture.Documents = [Page("Outside", "outside", outside)];

        var error = await Should.ThrowAsync<InvalidDataException>(() => fixture.BuildAsync());

        error.Message.ShouldContain("pages root");
        fixture.Renderer.Calls.ShouldBeEmpty();
    }

    [Fact]
    public async Task Given_CancellationAfterLoading_When_Generated_Then_It_Should_NotRenderASuccessfulSequence()
    {
        using var cancellation = new CancellationTokenSource();
        var fixture = new Fixture([Page("Page", "page", "page.md")]);
        fixture.OnLoad = cancellation.Cancel;

        await Should.ThrowAsync<OperationCanceledException>(() => fixture.BuildAsync(cancellationToken: cancellation.Token));

        fixture.Renderer.Calls.ShouldBeEmpty();
    }

    private static ContentDocument Page(string title, string slug, string source = "", bool visible = true) => new()
    {
        SourcePath = source,
        Kind = ContentKind.Page,
        Metadata = new()
        {
            Title = title,
            Slug = slug,
            ShowInNavigation = visible,
            Tags = visible && slug != "404.html" && !slug.StartsWith("hidden", StringComparison.Ordinal) ? ["sample"] : [],
        },
    };

    private sealed class Fixture
    {
        private readonly StaticSiteGenerator _generator;
        private readonly string _destination;

        public Fixture(IReadOnlyList<ContentDocument> documents)
        {
            Documents = documents;
            var fileSystem = new MockFileSystem();
            var root = Path.Combine(Path.GetPathRoot(Environment.CurrentDirectory)!, "base");
            var paths = new TestAppPaths(root, Path.Combine(root, "contents"), Path.Combine(root, "themes"));
            _destination = Path.Combine(root, "output");
            var loader = Substitute.For<IContentLoader>();
            loader.LoadAsync(Arg.Any<CancellationToken>()).Returns(_ =>
            {
                OnLoad?.Invoke();
                return Task.FromResult<IEnumerable<ContentDocument>>(Documents);
            });
            var markdown = Substitute.For<IMarkdownService>();
            markdown.ToHtmlAsync(Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>()).Returns("<p>Body</p>");
            Plugins.RunPreMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>())
                .Returns(call => Preprocess(call.ArgAt<ContentDocument>(0)));
            Plugins.RunPostMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>())
                .Returns(call => call.ArgAt<ContentDocument>(0));
            Plugins.RunPostHtmlAsync(Arg.Any<string>(), Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>())
                .Returns(call => call.ArgAt<string>(0));
            Plugins.Manifests.Returns([]);
            var themes = Substitute.For<IThemeService>();
            themes.LoadManifestAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new ThemeManifest());
            themes.CopyAssetsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            _generator = new StaticSiteGenerator(loader, markdown, Plugins, themes, Renderer, paths, fileSystem, Site, Substitute.For<ILogger<StaticSiteGenerator>>());
        }

        public IReadOnlyList<ContentDocument> Documents { get; set; }
        public Func<ContentDocument, ContentDocument> Preprocess { get; set; } = document => document;
        public Action? OnLoad { get; set; }
        public RecordingRenderer Renderer { get; } = new();
        public IPluginRunner Plugins { get; } = Substitute.For<IPluginRunner>();
        public SiteManifest Site { get; } = new() { BaseUrl = "/docs/" };

        public Task BuildAsync(bool preview = false, CancellationToken? cancellationToken = null)
            => _generator.BuildAsync<MainLayout, IndexView, PostView, PageView, NotFoundView, TagListView, TagView>(
                _destination, preview, cancellationToken ?? Xunit.TestContext.Current.CancellationToken);
    }

    private sealed class RecordingRenderer : IComponentRenderer
    {
        public List<RenderCall> Calls { get; } = [];

        public RenderCall ForDocument(ContentDocument document) => Calls.Single(call => ReferenceEquals(call.Document, document));

        public Task<string> RenderAsync<TComponent>(Type layoutType, IDictionary<string, object?> parameters, CancellationToken cancellationToken = default)
            where TComponent : IComponent
        {
            parameters.TryGetValue("Document", out var document);
            var pages = parameters["NavigationPages"];
            var tree = parameters["NavigationTree"];
            pages.ShouldNotBeNull();
            tree.ShouldNotBeNull();
            Calls.Add(new RenderCall(
                typeof(TComponent),
                document as ContentDocument,
                pages.ShouldBeAssignableTo<IReadOnlyList<ContentDocument>>(),
                tree.ShouldBeAssignableTo<IReadOnlyList<NavigationNode>>(),
                parameters["PageNavigation"].ShouldBeOfType<PageNavigation>(),
                new Dictionary<string, object?>(parameters)));
            return Task.FromResult("<p>Rendered</p>");
        }
    }

    private sealed record RenderCall(
        Type View,
        ContentDocument? Document,
        IReadOnlyList<ContentDocument> Pages,
        IReadOnlyList<NavigationNode> Tree,
        PageNavigation Navigation,
        IReadOnlyDictionary<string, object?> Parameters);
}
