using ScissorHands.Web.Abstractions;

namespace ScissorHands.Web.Tests.TestDoubles;

internal sealed class TestAppPaths : IAppPaths
{
    public TestAppPaths(string basePath, string contentsRoot, string themesRoot)
    {
        BasePath = basePath;
        _contentsRoot = contentsRoot;
        _themesRoot = themesRoot;
    }

    private readonly string _contentsRoot;
    private readonly string _themesRoot;

    public string BasePath { get; }

    public string GetContentsRoot() => _contentsRoot;

    public string GetThemesRoot() => _themesRoot;
}
