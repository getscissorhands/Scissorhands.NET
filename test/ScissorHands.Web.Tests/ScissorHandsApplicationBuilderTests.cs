using ScissorHands.Theme;

using TestTheme = ScissorHands.Theme.MinimalBlog;

namespace ScissorHands.Web.Tests;

[Collection("NonParallel")]
public class ScissorHandsApplicationBuilderTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Given_CompleteTheme_When_AddLayoutsInvoked_Then_It_Should_AcceptAllSevenRoles(bool generic)
    {
        var builder = new ScissorHandsApplicationBuilder(["--build"]);

        var result = generic
            ? builder.AddLayouts<TestTheme.MainLayout, TestTheme.IndexView, TestTheme.PostView, TestTheme.PageView, TestTheme.NotFoundView, TestTheme.TagListView, TestTheme.TagView>()
            : builder.AddLayouts(typeof(TestTheme.MainLayout), typeof(TestTheme.IndexView), typeof(TestTheme.PostView), typeof(TestTheme.PageView), typeof(TestTheme.NotFoundView), typeof(TestTheme.TagListView), typeof(TestTheme.TagView));

        result.ShouldBeSameAs(builder);
    }

    [Theory]
    [InlineData(null, typeof(ScissorHands.Web.TagView), "tagListView")]
    [InlineData(typeof(ScissorHands.Web.TagListView), null, "tagView")]
    public void Given_MissingTagRole_When_AddLayoutsInvoked_Then_It_Should_RejectTheRegistration(Type? tagListView, Type? tagView, string parameter)
    {
        var builder = new ScissorHandsApplicationBuilder(["--build"]);

        var exception = Should.Throw<ArgumentNullException>(() => Register(builder, tagListView, tagView));

        exception.ParamName.ShouldBe(parameter);
    }

    [Theory]
    [InlineData(typeof(TagListViewBase), typeof(ScissorHands.Web.TagView), "tagListView")]
    [InlineData(typeof(object), typeof(ScissorHands.Web.TagView), "tagListView")]
    [InlineData(typeof(ScissorHands.Web.TagListView), typeof(TagViewBase), "tagView")]
    [InlineData(typeof(ScissorHands.Web.TagListView), typeof(object), "tagView")]
    public void Given_InvalidTagRole_When_AddLayoutsInvoked_Then_It_Should_RejectTheRegistration(Type tagListView, Type tagView, string parameter)
    {
        var builder = new ScissorHandsApplicationBuilder(["--build"]);

        var exception = Should.Throw<ArgumentException>(() => Register(builder, tagListView, tagView));

        exception.ParamName.ShouldBe(parameter);
    }

    [Fact]
    public void Given_ListArgumentsAndNoExplicitLayouts_When_BuildInvoked_Then_It_Should_CreateApplication()
    {
        var builder = new ScissorHandsApplicationBuilder(new List<string> { "--build" });

        var application = builder.Build();

        application.ShouldNotBeNull();
    }

    [Fact]
    public async Task Given_RedirectedOutputAndHelpArgument_When_RunInvoked_Then_It_Should_DisplayHelpWithoutConsoleWindow()
    {
        var originalOutput = Console.Out;
        using var output = new StringWriter();

        try
        {
            Console.SetOut(output);
            var application = new ScissorHandsApplicationBuilder(["--help"]).Build();

            await application.RunAsync();

            output.ToString().ShouldContain("Usage: dotnet run");
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    private static IScissorHandsApplicationBuilder Register(ScissorHandsApplicationBuilder builder, Type? tagListView, Type? tagView)
        => builder.AddLayouts(typeof(MainLayout), typeof(IndexView), typeof(PostView), typeof(PageView), typeof(NotFoundView), tagListView!, tagView!);
}
