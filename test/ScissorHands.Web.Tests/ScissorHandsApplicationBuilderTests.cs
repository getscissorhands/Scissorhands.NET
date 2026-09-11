namespace ScissorHands.Web.Tests;

[Collection("NonParallel")]
public class ScissorHandsApplicationBuilderTests
{
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
}
