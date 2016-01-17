using System;

using FluentAssertions;

using Microsoft.AspNet.Http;

using Moq;

using Newtonsoft.Json;

using Scissorhands.Helpers.Tests.Fixtures;
using Scissorhands.Models.Posts;

using Xunit;

namespace Scissorhands.Helpers.Tests
{
    /// <summary>
    /// This represents the test entity for the <see cref="HttpRequestHelper"/> class.
    /// </summary>
    public class HttpClientHelperTest : IClassFixture<HttpClientHelperFixture>
    {
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly Mock<HttpRequest> _httpRequest;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientHelperTest"/> class.
        /// </summary>
        /// <param name="fixture"><see cref="HttpClientHelperFixture"/> instance.</param>
        public HttpClientHelperTest(HttpClientHelperFixture fixture)
        {
            this._httpRequestHelper = fixture.HttpClientHelper;
            this._httpRequest = fixture.HttpRequest;
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
            this._httpRequest.SetupGet(p => p.IsHttps).Returns(isHttps);
            this._httpRequest.SetupGet(p => p.Host).Returns(new HostString(host));

            var url = string.Join("://", isHttps ? "https" : "http", host);

            var client = this._httpRequestHelper.CreateHttpClient(this._httpRequest.Object);
            client.BaseAddress.ToString().TrimEnd('/').Should().Be(url);
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
        [InlineData("**Hello World", "<strong>Hello World</strong>")]
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