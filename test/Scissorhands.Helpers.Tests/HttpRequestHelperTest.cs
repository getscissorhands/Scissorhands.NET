using System;

using FluentAssertions;

using Microsoft.AspNet.Http;

using Moq;

using Newtonsoft.Json;

using Scissorhands.Extensions;
using Scissorhands.Helpers.Tests.Fixtures;
using Scissorhands.Models.Posts;
using Scissorhands.Models.Settings;
using Scissorhands.ViewModels.Post;

using Xunit;

namespace Scissorhands.Helpers.Tests
{
    /// <summary>
    /// This represents the test entity for the <see cref="HttpRequestHelper"/> class.
    /// </summary>
    public class HttpRequestHelperTest : IClassFixture<HttpRequestHelperFixture>
    {
        private readonly string _baseUrl;
        private readonly string _basePath;
        private readonly Mock<SiteMetadataSettings> _metadata;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly Mock<HttpRequest> _httpRequest;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestHelperTest"/> class.
        /// </summary>
        /// <param name="fixture"><see cref="HttpRequestHelperFixture"/> instance.</param>
        public HttpRequestHelperTest(HttpRequestHelperFixture fixture)
        {
            this._baseUrl = fixture.BaseUrl;
            this._basePath = fixture.BasePath;
            this._metadata = fixture.SiteMetadataSettings;
            this._httpRequestHelper = fixture.HttpClientHelper;
            this._httpRequest = fixture.HttpRequest;
        }

        /// <summary>
        /// Tests whether constructor should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_NullParameter_Constructor_ShouldThrow_ArgumentNullException()
        {
            Action action = () => { var constructor = new HttpRequestHelper(null); };
            action.ShouldThrow<ArgumentNullException>();
        }

        /// <summary>
        /// Tests whether constructor should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_Parameter_Constructor_ShouldThrow_NoException()
        {
            Action action = () => { var constructor = new HttpRequestHelper(this._metadata.Object); };
            action.ShouldNotThrow<Exception>();
        }

        /// <summary>
        /// Tests whether GetBaseUri should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_NullRequest_GetBaseUri_ShouldThrow_ArgumentNullException()
        {
            Action action = () => { var result = this._httpRequestHelper.GetBaseUri(null, PublishMode.Undefined); };
            action.ShouldThrow<ArgumentNullException>();
        }

        /// <summary>
        /// Tests whether GetBaseUri should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_UndefinedMode_GetBaseUri_ShouldThrow_InvalidOperationException()
        {
            Action action = () => { var result = this._httpRequestHelper.GetBaseUri(this._httpRequest.Object, PublishMode.Undefined); };
            action.ShouldThrow<InvalidOperationException>();
        }

        /// <summary>
        /// Tests whether GetBaseUri should return result or not.
        /// </summary>
        /// <param name="isHttps">Value that indicates whether HTTPS connection or not.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Given_PreviewMode_GetBaseUri_ShouldReturn_Result(bool isHttps)
        {
            var hostString = HostString.FromUriComponent(this._baseUrl);
            var url = string.Join("://", isHttps ? "https" : "http", this._baseUrl);
            var uri = new Uri(url);

            this._httpRequest.SetupGet(p => p.IsHttps).Returns(isHttps);
            this._httpRequest.SetupGet(p => p.Host).Returns(hostString);

            var result = this._httpRequestHelper.GetBaseUri(this._httpRequest.Object, PublishMode.Preview);
            result.Should().Be(uri);
        }

        /// <summary>
        /// Tests whether GetBaseUri should return result or not.
        /// </summary>
        [Fact]
        public void Given_PublishMode_GetBaseUri_ShouldReturn_Result()
        {
            var uri = new Uri(this._baseUrl);
            var result = this._httpRequestHelper.GetBaseUri(this._httpRequest.Object, PublishMode.Publish);
            result.Should().Be(uri);
        }

        /// <summary>
        /// Tests whether GetSlugPrefix should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_NullRequest_GetSlugPrefix_ShouldThrow_ArgumentNullException()
        {
            Action action = () => { var result = this._httpRequestHelper.GetSlugPrefix(null); };
            action.ShouldThrow<ArgumentNullException>();
        }

        /// <summary>
        /// Tests whether GetSlugPrefix should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_UndefinedMode_GetSlugPrefix_ShouldThrow_InvalidOperationException()
        {
            Action action = () => { var result = this._httpRequestHelper.GetSlugPrefix(this._httpRequest.Object, PublishMode.Undefined); };
            action.ShouldThrow<InvalidOperationException>();
        }

        /// <summary>
        /// Tests whether GetSlugPrefix should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_UndefinedPageType_GetSlugPrefix_ShouldThrow_InvalidOperationException()
        {
            Action action = () => { var result = this._httpRequestHelper.GetSlugPrefix(this._httpRequest.Object, PublishMode.Publish, PageType.Undefined); };
            action.ShouldThrow<InvalidOperationException>();
        }

        [Theory]
        [InlineData(PageType.Post, "/posts")]
        [InlineData(PageType.Page, "/pages")]
        public void Given_PageType_GetSlugPrefix_ShouldReturn_Value(PageType pageType, string segment)
        {
            var expected = pageType == PageType.Page ? segment : $"{segment}/{DateTime.Today.ToString("yyyy/MM/dd")}";

            var result = this._httpRequestHelper.GetSlugPrefix(this._httpRequest.Object, PublishMode.Publish, pageType);

            result.Should().EndWithEquivalent(expected);
        }

        /// <summary>
        /// Tests whether GetSlugPrefix should return result or not.
        /// </summary>
        /// <param name="isHttps">Value that indicates whether HTTPS connection or not.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Given_PreviewMode_GetSlugPrefix_ShouldReturn_Result(bool isHttps)
        {
            var hostString = HostString.FromUriComponent(this._baseUrl);
            var url = string.Join("://", isHttps ? "https" : "http", this._baseUrl);
            var uri = new Uri(url);
            var today = $"{DateTime.Today.ToString("yyyy/MM/dd")}";

            var expected = $"{uri.TrimTrailingSlash()}{this._basePath.OrRootPath().TrimTrailingSlash()}/posts/{today}";

            this._httpRequest.SetupGet(p => p.IsHttps).Returns(isHttps);
            this._httpRequest.SetupGet(p => p.Host).Returns(hostString);

            var result = this._httpRequestHelper.GetSlugPrefix(this._httpRequest.Object, PublishMode.Preview);
            result.Should().BeEquivalentTo(expected);
        }

        /// <summary>
        /// Tests whether the method should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_NullHttpRequest_CreateHttpClient_ShouldThrow_ArgumentNullException()
        {
            Action action = () => { var client = this._httpRequestHelper.CreateHttpClient(null); };
            action.ShouldThrow<ArgumentNullException>();
        }

        /// <summary>
        /// Tests whether the method should return result or not.
        /// </summary>
        /// <param name="isHttps">Value that indicates whether HTTPS connection or not.</param>
        /// <param name="host">Host name and port.</param>
        [Theory]
        [InlineData(false, "localhost:5080")]
        [InlineData(true, "localhost:5080")]
        public void Given_HttpRequest_CreateHttpClient_ShouldReturn_HttpClient(bool isHttps, string host)
        {
            var url = string.Join("://", isHttps ? "https" : "http", host);

            this._httpRequest.SetupGet(p => p.IsHttps).Returns(isHttps);
            this._httpRequest.SetupGet(p => p.Host).Returns(new HostString(host));
            this._metadata.SetupGet(p => p.BaseUrl).Returns(url);

            var client = this._httpRequestHelper.CreateHttpClient(this._httpRequest.Object);
            client.BaseAddress.TrimTrailingSlash().Should().Be(url);
        }

        /// <summary>
        /// Tests whether the method should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_NullModel_CreateStringContent_ShouldThrow_ArgumentNullException()
        {
            Action action = () => { var content = this._httpRequestHelper.CreateStringContent(null); };
            action.ShouldThrow<ArgumentNullException>();
        }

        /// <summary>
        /// Tests whether the method should return result or not.
        /// </summary>
        /// <param name="markdown">Markdown value.</param>
        /// <param name="html">HTML value.</param>
        [Theory]
        [InlineData("**Hello World**", "<strong>Hello World</strong>")]
        public async void Given_Model_CreateStringContent_ShouldReturn_StringContent(string markdown, string html)
        {
            var model = new PublishedContent() { Theme = "default", Markdown = markdown, Html = html };

            var content = this._httpRequestHelper.CreateStringContent(model);
            var result = JsonConvert.DeserializeObject<PublishedContent>(await content.ReadAsStringAsync().ConfigureAwait(false));

            result.Markdown.Should().Be(markdown);
            result.Html.Should().Be(html);
        }
    }
}