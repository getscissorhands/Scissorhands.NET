using System.IO;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Scissorhands.Core.Resolvers;
using Scissorhands.Core.Tests.Utilities;

namespace Scissorhands.Core.Tests.Resolvers
{
    [TestClass]
    public class ContentsResolverTests
    {
        private TestSettings _settings;
        private string _contentsRootDirectory;

        [TestInitialize]
        public async Task Init()
        {
            this._settings = await TestSettings.LoadAsync().ConfigureAwait(false);
            this._contentsRootDirectory = Path.Combine(this._settings.ProjectDirectory.TrimEnd(Path.DirectorySeparatorChar), Constants.Contents);
        }

        [TestMethod]
        public void Given_ContentRootDirectory_Then_It_Should_Return_Directories()
        {
            var directories = new DirectoryInfo(this._contentsRootDirectory).GetDirectories(Constants.AllDirectories, SearchOption.TopDirectoryOnly);

            directories.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public void Given_ContentRootDirectory_Then_It_Should_Return_Files()
        {
            var files = new DirectoryInfo(this._contentsRootDirectory).GetFiles(Constants.AllMarkdownFiles, SearchOption.TopDirectoryOnly);

            files.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public void Given_Null_When_Instantiated_Then_It_Should_Return_Result()
        {
            var resolver = new ContentsResolver();

            resolver.ParentDirectory.Should().BeNullOrWhiteSpace();
        }

        [TestMethod]
        public void Given_ContentRootDirectory_When_Instantiated_Then_It_Should_Return_Result()
        {
            var resolver = new ContentsResolver(this._contentsRootDirectory);

            resolver.ParentDirectory.Should().BeEquivalentTo(this._contentsRootDirectory);
        }

        [TestMethod]
        public async Task Given_ContentRootDirectory_When_ResolveAsync_Invoked_Then_It_Should_Return_Result()
        {
            var resolver = new ContentsResolver(this._contentsRootDirectory);

            var result = await resolver.ResolveAsync().ConfigureAwait(false);

            result.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public async Task Given_ContentRootDirectory_When_ResolveAsync_Invoked_Then_It_Should_Return_Index()
        {
            var resolver = new ContentsResolver(this._contentsRootDirectory);

            var result = await resolver.ResolveAsync().ConfigureAwait(false);
            var index = result.First();

            index.UrlPath.Should().Be("/");
        }

        [DataTestMethod]
        [DataRow(1, "/page-1")]
        [DataRow(2, "/page-2")]
        public async Task Given_ContentRootDirectory_When_ResolveAsync_Invoked_Then_It_Should_Return_Page(int skip, string slug)
        {
            var resolver = new ContentsResolver(this._contentsRootDirectory);

            var result = await resolver.ResolveAsync().ConfigureAwait(false);
            var page = result.Skip(skip + 1).Take(1).First();

            page.UrlPath.Should().Be(slug);
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(2)]
        public async Task Given_ContentRootDirectory_When_ResolveAsync_Invoked_Then_It_Should_Return_ChildPages(int skip)
        {
            var resolver = new ContentsResolver(this._contentsRootDirectory);

            var result = await resolver.ResolveAsync().ConfigureAwait(false);
            var page = result.Skip(skip + 1).Take(1).First();

            page.ChildItems.Should().NotBeNullOrEmpty();
        }

        [DataTestMethod]
        [DataRow(0, "/page-1/subpage-1-1")]
        [DataRow(1, "/my-subpage-1-2")]
        public async Task Given_ContentRootDirectory_When_ResolveAsync_Invoked_Then_It_Should_Return_ChildPage(int skip, string slug)
        {
            var resolver = new ContentsResolver(this._contentsRootDirectory);

            var result = await resolver.ResolveAsync().ConfigureAwait(false);
            var page = result.Skip(2).Take(1).First();
            var subpage = page.ChildItems.Skip(skip).Take(1).First();

            subpage.UrlPath.Should().Be(slug);
        }
    }
}