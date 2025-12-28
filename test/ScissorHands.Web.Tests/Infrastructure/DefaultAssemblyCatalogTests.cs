using ScissorHands.Web.Infrastructure;

namespace ScissorHands.Web.Tests.Infrastructure;

public class DefaultAssemblyCatalogTests
{
    [Fact]
    public void Given_DefaultAssemblyCatalog_When_GetAssembliesInvoked_Then_It_Should_IncludeCurrentAssemblyAndReturnDistinct()
    {
        var catalog = new DefaultAssemblyCatalog();

        var assemblies = catalog.GetAssemblies();

        assemblies.ShouldNotBeEmpty();
        assemblies.ShouldContain(typeof(DefaultAssemblyCatalog).Assembly);

        assemblies.Distinct().Count().ShouldBe(assemblies.Count);
    }
}
