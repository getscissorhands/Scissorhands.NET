using System;
using System.IO;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Scissorhands.Core.Tests.Utilities;

namespace Scissorhands.Core.Tests
{
    [TestClass]
    public class ContentItemTests
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
        public void Given_Null_When_Instantiated_Then_It_Should_Throw_Exception()
        {
            Action action = () => new ContentItem(null);

            action.Should().Throw<ArgumentNullException>();
        }

        [DataTestMethod]
        [DataRow("index.md")]
        [DataRow("page-1/subpage-1-1.md")]
        public void Given_FilePath_When_Instantiated_Then_It_Should_Return_Result(string filepath)
        {
            filepath = Path.Combine(this._contentsRootDirectory, filepath);

            var result = new ContentItem(filepath);

            result.FilePath.Should().Be(filepath);
            result.ChildItems.Should().NotBeNull();
            result.ChildItems.Should().BeEmpty();
        }

        [DataTestMethod]
        [DataRow("index.md", false)]
        [DataRow("page-1.md", true)]
        public void Given_Directories_When_IsDirectory_Invoked_Then_It_Should_Return_Result(string filepath, bool expected)
        {
            filepath = Path.Combine(this._contentsRootDirectory, filepath);
            var contentItem = new ContentItem(filepath);

            var directories = new DirectoryInfo(this._contentsRootDirectory).GetDirectories(Constants.AllDirectories, SearchOption.TopDirectoryOnly);

            var result = contentItem.IsDirectory(directories);

            result.Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow("index.md", "/", 0)]
        [DataRow("page-1.md", "/page-1", 101)]
        public async Task Given_FilePath_When_LoadContentAsync_Invoked_Then_It_Should_Return_Result(string filepath, string slug, int order)
        {
            filepath = Path.Combine(this._contentsRootDirectory, filepath);
            var contentItem = new ContentItem(filepath);

            await contentItem.LoadContentAsync().ConfigureAwait(false);

            contentItem.RawBody.Should().NotBeNullOrWhiteSpace();
            contentItem.FrontMatter.Should().NotBeNull();
            contentItem.MarkdownBody.Should().NotBeNullOrWhiteSpace();
            contentItem.HtmlBody.Should().NotBeNullOrWhiteSpace();
            contentItem.UrlPath.Should().Be(slug);
            contentItem.Order.Should().Be(order);
        }
    }
}