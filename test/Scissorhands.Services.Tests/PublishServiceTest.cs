using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.AspNet.Http;
using Microsoft.Extensions.PlatformAbstractions;

using Moq;

using Scissorhands.Helpers;
using Scissorhands.Models.Settings;
using Scissorhands.Services.Exceptions;
using Scissorhands.Services.Tests.Fakes;
using Scissorhands.Services.Tests.Fixtures;

using Xunit;

namespace Scissorhands.Services.Tests
{
    /// <summary>
    /// This represents the test entity for the <see cref="PublishService"/> class.
    /// </summary>
    public class PublishServiceTest : IClassFixture<PublishServiceFixture>
    {
        private readonly Mock<WebAppSettings> _settings;
        private readonly Mock<SiteMetadataSettings> _metadata;
        private readonly Mock<IMarkdownHelper> _markdownHelper;
        private readonly Mock<IFileHelper> _fileHelper;
        private readonly Mock<IHttpRequestHelper> _httpRequestHelper;
        private readonly IPublishService _service;

        private readonly Mock<IApplicationEnvironment> _env;
        private readonly string _filepath;
        private readonly Mock<HttpRequest> _request;
        private readonly string _defaultThemeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishServiceTest"/> class.
        /// </summary>
        /// <param name="fixture"><see cref="PublishServiceFixture"/> instance.</param>
        public PublishServiceTest(PublishServiceFixture fixture)
        {
            this._settings = fixture.WebAppSettings;
            this._metadata = fixture.SiteMetadataSettings;
            this._markdownHelper = fixture.MarkdownHelper;
            this._fileHelper = fixture.FileHelper;
            this._httpRequestHelper = fixture.HttpRequestHelper;
            this._service = fixture.PublishService;

            this._env = new Mock<IApplicationEnvironment>();
            this._filepath = "/home/scissorhands.net/wwwroot/posts".Replace('/', Path.DirectorySeparatorChar);
            this._request = new Mock<HttpRequest>();
            this._defaultThemeName = "default";
        }

        /// <summary>
        /// Tests whether constructor should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_NullParameter_Constructor_ShouldThrow_ArgumentNullException()
        {
            Action action1 = () => { var service = new PublishService(null, this._metadata.Object,  this._markdownHelper.Object, this._fileHelper.Object, this._httpRequestHelper.Object); };
            action1.ShouldThrow<ArgumentNullException>();

            Action action2 = () => { var service = new PublishService(this._settings.Object, null, this._markdownHelper.Object, this._fileHelper.Object, this._httpRequestHelper.Object); };
            action2.ShouldThrow<ArgumentNullException>();

            Action action3 = () => { var service = new PublishService(this._settings.Object, this._metadata.Object, null, this._fileHelper.Object, this._httpRequestHelper.Object); };
            action3.ShouldThrow<ArgumentNullException>();

            Action action4 = () => { var service = new PublishService(this._settings.Object, this._metadata.Object, this._markdownHelper.Object, null, this._httpRequestHelper.Object); };
            action4.ShouldThrow<ArgumentNullException>();

            Action action5 = () => { var service = new PublishService(this._settings.Object, this._metadata.Object, this._markdownHelper.Object, this._fileHelper.Object, null); };
            action5.ShouldThrow<ArgumentNullException>();
        }

        /// <summary>
        /// Tests whether constructor should NOT throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_Parameters_Constructor_ShouldThrow_NoException()
        {
            Action action = () => { var service = new PublishService(this._settings.Object, this._metadata.Object, this._markdownHelper.Object, this._fileHelper.Object, this._httpRequestHelper.Object); };
            action.ShouldNotThrow<Exception>();
        }

        /// <summary>
        /// Tests whether the method should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_NullMarkdown_PublishMarkdownAsync_ShouldThrow_ArgumentNullException()
        {
            Func<Task> func = async () => { var result = await this._service.PublishMarkdownAsync(null).ConfigureAwait(false); };
            func.ShouldThrow<ArgumentNullException>();
        }

        /// <summary>
        /// Tests whether the method should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_FalseWritingSync_PublishMarkdownAsync_ShouldThrow_PublishFailedException()
        {
            this._fileHelper.Setup(p => p.GetDirectory(It.IsAny<string>())).Returns(this._filepath);
            this._fileHelper.Setup(p => p.WriteAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(false));

            Func<Task> func = async () => { var result = await this._service.PublishMarkdownAsync("**Hello World**").ConfigureAwait(false); };
            func.ShouldThrow<PublishFailedException>();
        }

        /// <summary>
        /// Tests whether the method should return value or not.
        /// </summary>
        /// <param name="markdownpath">File path.</param>
        [Theory]
        [InlineData("/posts/markdown.md")]
        public async void Given_Markdown_PublishMarkdownAsync_ShouldReturn_Filepath(string markdownpath)
        {
            this._fileHelper.Setup(p => p.GetDirectory(It.IsAny<string>())).Returns(this._filepath);
            this._fileHelper.Setup(p => p.WriteAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));

            var result = await this._service.PublishMarkdownAsync("**Hello World**").ConfigureAwait(false);
            result.Should().Be(markdownpath);
        }

        /// <summary>
        /// Tests whether the method should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_NullHtml_PublishHtmlAsync_ShouldThrow_ArgumentNullException()
        {
            Func<Task> func = async () => { var result = await this._service.PublishHtmlAsync(null).ConfigureAwait(false); };
            func.ShouldThrow<ArgumentNullException>();
        }

        /// <summary>
        /// Tests whether the method should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_FalseWritingSync_PublishHtmlAsync_ShouldThrow_PublishFailedException()
        {
            this._fileHelper.Setup(p => p.GetDirectory(It.IsAny<string>())).Returns(this._filepath);
            this._fileHelper.Setup(p => p.WriteAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(false));

            Func<Task> func = async () => { var result = await this._service.PublishHtmlAsync("**Hello World**").ConfigureAwait(false); };
            func.ShouldThrow<PublishFailedException>();
        }

        /// <summary>
        /// Tests whether the method should return value or not.
        /// </summary>
        /// <param name="htmlpath">File path.</param>
        [Theory]
        [InlineData("/posts/post.html")]
        public async void Given_Markdown_PublishHtmlAsync_ShouldReturn_Filepath(string htmlpath)
        {
            this._fileHelper.Setup(p => p.GetDirectory(It.IsAny<string>())).Returns(this._filepath);
            this._fileHelper.Setup(p => p.WriteAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));

            var result = await this._service.PublishHtmlAsync("<strong>Hello World</strong>").ConfigureAwait(false);
            result.Should().Be(htmlpath);
        }

        /// <summary>
        /// Tests whether the method should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_NullParameter_GetPublishedHtmlAsync_ShouldThrow_ArgumentNullException()
        {
            Func<Task> func1 = async () => { var html = await this._service.GetPublishedHtmlAsync(null, this._request.Object).ConfigureAwait(false); };
            func1.ShouldThrow<ArgumentNullException>();

            var markdown = "**Hello World**";
            Func<Task> func2 = async () => { var html = await this._service.GetPublishedHtmlAsync(markdown, null).ConfigureAwait(false); };
            func2.ShouldThrow<ArgumentNullException>();
        }

        /// <summary>
        /// Tests whether the method should return result or not.
        /// </summary>
        /// <param name="markdown">Markdown string.</param>
        /// <param name="html">HTML string.</param>
        [Theory]
        [InlineData("**Hello World**", "<strong>Joe Bloggs</strong>")]
        public async void Given_Parameters_GetPublishedHtmlAsync_ShouldReturn_Html(string markdown, string html)
        {
            this._markdownHelper.Setup(p => p.Parse(It.IsAny<string>())).Returns(html);
            this._metadata.SetupGet(p => p.Theme).Returns(this._defaultThemeName);

            var message = new HttpResponseMessage { Content = new StringContent(html) };

            var handler = new HttpResponseHandlerFake(message);
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5080") };

            this._httpRequestHelper.Setup(p => p.CreateHttpClient(It.IsAny<HttpRequest>(), It.IsAny<HttpMessageHandler>())).Returns(client);

            var content = new StringContent(html, Encoding.UTF8);
            this._httpRequestHelper.Setup(p => p.CreateStringContent(It.IsAny<object>())).Returns(content);

            var result = await this._service.GetPublishedHtmlAsync(markdown, this._request.Object).ConfigureAwait(false);
            result.Should().Be(html);
        }

        /// <summary>
        /// Tests whether the method should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_NullParameter_PublishPostAsync_ShouldThrow_ArgumentNullException()
        {
            var markdown = "**Hello World**";

            Func<Task> func1 = async () => { var result = await this._service.PublishPostAsync(null, this._env.Object, this._request.Object).ConfigureAwait(false); };
            func1.ShouldThrow<ArgumentNullException>();

            Func<Task> func2 = async () => { var result = await this._service.PublishPostAsync(markdown, null, this._request.Object).ConfigureAwait(false); };
            func2.ShouldThrow<ArgumentNullException>();

            Func<Task> func3 = async () => { var result = await this._service.PublishPostAsync(markdown, this._env.Object, null).ConfigureAwait(false); };
            func3.ShouldThrow<ArgumentNullException>();
        }

        /// <summary>
        /// Tests whether the method should return result or not.
        /// </summary>
        /// <param name="markdownpath">Markdown file path.</param>
        /// <param name="htmlpath">HTML file path.</param>
        [Theory]
        [InlineData("/posts/markdown.md", "/posts/post.html")]
        public async void Given_Parameters_PublishPostAsync_ShouldReturn_Result(string markdownpath, string htmlpath)
        {
            var markdown = "**Hello World**";
            var html = "<strong>Joe Bloggs</strong>";

            this._fileHelper.Setup(p => p.GetDirectory(It.IsAny<string>())).Returns(this._filepath);
            this._fileHelper.Setup(p => p.WriteAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));

            this._markdownHelper.Setup(p => p.Parse(It.IsAny<string>())).Returns(html);
            this._metadata.SetupGet(p => p.Theme).Returns(this._defaultThemeName);

            var message = new HttpResponseMessage { Content = new StringContent(html) };

            var handler = new HttpResponseHandlerFake(message);
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5080") };

            this._httpRequestHelper.Setup(p => p.CreateHttpClient(It.IsAny<HttpRequest>(), It.IsAny<HttpMessageHandler>())).Returns(client);

            var content = new StringContent(html, Encoding.UTF8);
            this._httpRequestHelper.Setup(p => p.CreateStringContent(It.IsAny<object>())).Returns(content);

            var publishedpath = await this._service.PublishPostAsync(markdown, this._env.Object, this._request.Object).ConfigureAwait(false);
            publishedpath.Markdown.Should().BeEquivalentTo(markdownpath);
            publishedpath.Html.Should().BeEquivalentTo(htmlpath);
        }
    }
}