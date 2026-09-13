using System.Reflection;

using ScissorHands.Theme;
using ScissorHands.Web.Abstractions;
using ScissorHands.Web.Services;

namespace ScissorHands.Web.Tests.Services;

public class ThemeComponentResolverTests
{
    [Fact]
    public void Given_DefaultTheme_When_ResolveInvoked_Then_It_Should_ReturnBuiltInComponents()
    {
        var catalog = Substitute.For<IAssemblyCatalog>();
        catalog.GetAssemblies().Returns([typeof(ScissorHands.Web.MainLayout).Assembly]);
        var resolver = new ThemeComponentResolver(catalog);

        var components = resolver.Resolve("default");

        components.MainLayout.ShouldBe(typeof(ScissorHands.Web.MainLayout));
        components.IndexView.ShouldBe(typeof(ScissorHands.Web.IndexView));
        components.TagListView.ShouldBe(typeof(ScissorHands.Web.TagListView));
        components.TagView.ShouldBe(typeof(ScissorHands.Web.TagView));
    }

    [Fact]
    public void Given_ExternalThemeSlug_When_ResolveInvoked_Then_It_Should_MatchAllSevenThemeComponents()
    {
        var catalog = Substitute.For<IAssemblyCatalog>();
        catalog.GetAssemblies().Returns(
        [
            typeof(ScissorHands.Web.MainLayout).Assembly,
            typeof(ScissorHands.Theme.MinimalBlog.MainLayout).Assembly,
        ]);
        var resolver = new ThemeComponentResolver(catalog);

        var components = resolver.Resolve("minimal-blog");

        components.MainLayout.ShouldBe(typeof(ScissorHands.Theme.MinimalBlog.MainLayout));
        components.IndexView.ShouldBe(typeof(ScissorHands.Theme.MinimalBlog.IndexView));
        components.TagListView.ShouldBe(typeof(ScissorHands.Theme.MinimalBlog.TagListView));
        components.TagView.ShouldBe(typeof(ScissorHands.Theme.MinimalBlog.TagView));
    }

    [Fact]
    public void Given_ThemeTemplateSlug_When_ResolveInvoked_Then_It_Should_MatchExistingTemplateNamespace()
    {
        var catalog = Substitute.For<IAssemblyCatalog>();
        catalog.GetAssemblies().Returns(
        [
            typeof(ScissorHands.Web.MainLayout).Assembly,
            typeof(ScissorHands.Theme.Template.MainLayout).Assembly,
        ]);
        var resolver = new ThemeComponentResolver(catalog);

        var components = resolver.Resolve("theme-template");

        components.MainLayout.ShouldBe(typeof(ScissorHands.Theme.Template.MainLayout));
        components.IndexView.ShouldBe(typeof(ScissorHands.Theme.Template.IndexView));
        components.TagListView.ShouldBe(typeof(ScissorHands.Theme.Template.TagListView));
        components.TagView.ShouldBe(typeof(ScissorHands.Theme.Template.TagView));
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public void Given_MissingTagRoles_When_ResolveInvoked_Then_It_Should_RejectTheThemeWithoutUsingBuiltIns(bool includeTagList, bool includeTag)
    {
        var types = CompleteThemeTypes()
            .Where(type => includeTagList || type != typeof(ScissorHands.Theme.MinimalBlog.TagListView))
            .Where(type => includeTag || type != typeof(ScissorHands.Theme.MinimalBlog.TagView))
            .ToArray();
        var resolver = CreateResolver(types);

        var exception = Should.Throw<InvalidOperationException>(() => resolver.Resolve("minimal-blog"));

        exception.Message.ShouldContain("minimal-blog");
        exception.Message.ShouldContain("TagListView");
        exception.Message.ShouldContain("TagView");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Given_DuplicateTagRole_When_ResolveInvoked_Then_It_Should_RejectTheAmbiguousTheme(bool duplicateTagList)
    {
        var duplicate = duplicateTagList ? typeof(ScissorHands.Theme.MinimalBlog.TagListView) : typeof(ScissorHands.Theme.MinimalBlog.TagView);
        var resolver = CreateResolver([.. CompleteThemeTypes(), duplicate]);

        var exception = Should.Throw<InvalidOperationException>(() => resolver.Resolve("minimal-blog"));

        exception.Message.ShouldContain("exactly one concrete component");
        exception.Message.ShouldContain("minimal-blog");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Given_AbstractTagRole_When_ResolveInvoked_Then_It_Should_RejectTheIncompleteTheme(bool abstractTagList)
    {
        var types = CompleteThemeTypes()
            .Select(type => abstractTagList && type == typeof(ScissorHands.Theme.MinimalBlog.TagListView)
                ? typeof(TagListViewBase)
                : !abstractTagList && type == typeof(ScissorHands.Theme.MinimalBlog.TagView) ? typeof(TagViewBase) : type)
            .ToArray();
        var resolver = CreateResolver(types);

        Should.Throw<InvalidOperationException>(() => resolver.Resolve("minimal-blog"));
    }

    [Fact]
    public void Given_OnlyIncompleteThemes_When_ResolveInvoked_Then_It_Should_DescribeAllRequiredRoles()
    {
        var assembly = Substitute.For<Assembly>();
        assembly.GetTypes().Returns(CompleteThemeTypes().Take(5).ToArray());
        var catalog = Substitute.For<IAssemblyCatalog>();
        catalog.GetAssemblies().Returns([assembly]);

        var exception = Should.Throw<InvalidOperationException>(() => new ThemeComponentResolver(catalog).Resolve("minimal-blog"));

        exception.Message.ShouldContain("No complete");
        exception.Message.ShouldContain("minimal-blog");
        exception.Message.ShouldContain("TagListView");
        exception.Message.ShouldContain("TagView");
    }

    [Fact]
    public void Given_UnmatchedThemeSlug_When_ResolveInvoked_Then_It_Should_NotSilentlySelectAnotherTheme()
    {
        var catalog = Substitute.For<IAssemblyCatalog>();
        catalog.GetAssemblies().Returns(
        [
            typeof(ScissorHands.Web.MainLayout).Assembly,
            typeof(ScissorHands.Theme.MinimalBlog.MainLayout).Assembly,
        ]);
        var resolver = new ThemeComponentResolver(catalog);

        var exception = Should.Throw<InvalidOperationException>(() => resolver.Resolve("missing-theme"));

        exception.Message.ShouldContain("Unable to resolve");
        exception.Message.ShouldContain("missing-theme");
    }

    private static ThemeComponentResolver CreateResolver(Type[] types)
    {
        var assembly = Substitute.For<Assembly>();
        assembly.GetTypes().Returns(types);
        var catalog = Substitute.For<IAssemblyCatalog>();
        catalog.GetAssemblies().Returns([typeof(ScissorHands.Web.MainLayout).Assembly, assembly]);
        return new ThemeComponentResolver(catalog);
    }

    private static Type[] CompleteThemeTypes() =>
    [
        typeof(ScissorHands.Theme.MinimalBlog.MainLayout),
        typeof(ScissorHands.Theme.MinimalBlog.IndexView),
        typeof(ScissorHands.Theme.MinimalBlog.PostView),
        typeof(ScissorHands.Theme.MinimalBlog.PageView),
        typeof(ScissorHands.Theme.MinimalBlog.NotFoundView),
        typeof(ScissorHands.Theme.MinimalBlog.TagListView),
        typeof(ScissorHands.Theme.MinimalBlog.TagView),
    ];
}
