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
        components.TagView.ShouldBe(typeof(ScissorHands.Web.TagView));
    }

    [Fact]
    public void Given_ExternalThemeSlug_When_ResolveInvoked_Then_It_Should_MatchNormalizedNamespace_And_FillOptionalTagViews()
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
        components.TagListView.ShouldBe(typeof(ScissorHands.Web.TagListView));
        components.TagView.ShouldBe(typeof(ScissorHands.Web.TagView));
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
}
