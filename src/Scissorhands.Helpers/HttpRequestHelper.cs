using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

using Microsoft.AspNet.Http;

using Newtonsoft.Json;
using Scissorhands.Extensions;
using Scissorhands.Models.Settings;
using Scissorhands.ViewModels.Post;

namespace Scissorhands.Helpers
{
    /// <summary>
    /// This represents the helper entity for <see cref="HttpClient"/> and its related ones.
    /// </summary>
    public class HttpRequestHelper : IHttpRequestHelper
    {
        private const string MediaType = "application/json";
        private const string CharSet = "utf-8";

        private readonly SiteMetadataSettings _metadata;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestHelper"/> class.
        /// </summary>
        /// <param name="metadata"><see cref="SiteMetadataSettings"/> instance.</param>
        public HttpRequestHelper(SiteMetadataSettings metadata)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            this._metadata = metadata;
        }

        /// <summary>
        /// Gets the <see cref="Uri"/> instance representing base URI.
        /// </summary>
        /// <param name="request"><see cref="HttpRequest"/> instance.</param>
        /// <param name="mode"><see cref="PublishMode"/> value.</param>
        /// <returns>Returns the <see cref="Uri"/> instance representing base URI.</returns>
        public Uri GetBaseUri(HttpRequest request, PublishMode mode)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            Uri uri;
            switch (mode)
            {
                case PublishMode.Preview:
                    uri = GetBaseUri(request);
                    break;

                case PublishMode.Publish:
                    uri = new Uri(this._metadata.BaseUrl);
                    break;

                default:
                    throw new InvalidOperationException("Invalid publish mode");
            }

            return uri;
        }

        /// <summary>
        /// Gets the prefix of the slug.
        /// </summary>
        /// <param name="request"><see cref="HttpRequest"/> instance.</param>
        /// <param name="pageType"><see cref="PageType"/> value.</param>
        /// <returns>Returns the prefix of the slug.</returns>
        public string GetSlugPrefix(HttpRequest request, PageType? pageType = null)
        {
            return this.GetSlugPrefix(request, PublishMode.Publish, pageType);
        }

        /// <summary>
        /// Gets the prefix of the slug.
        /// </summary>
        /// <param name="request"><see cref="HttpRequest"/> instance.</param>
        /// <param name="mode"><see cref="PublishMode"/> value.</param>
        /// <param name="pageType"><see cref="PageType"/> value.</param>
        /// <returns>Returns the prefix of the slug.</returns>
        public string GetSlugPrefix(HttpRequest request, PublishMode mode, PageType? pageType = null)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (pageType == null)
            {
                pageType = PageType.Post;
            }

            var baseUri = this.GetBaseUri(request, mode);
            var basePath = this._metadata.BasePath.OrRootPath().TrimTrailingSlash();
            var today = $"{DateTime.Today.ToString("yyyy/MM/dd")}";

            var prefix = $"{baseUri.TrimTrailingSlash()}{basePath}";
            switch (pageType.GetValueOrDefault(PageType.Undefined))
            {
                case PageType.Post:
                    prefix += $"/posts/{today}";
                    break;

                case PageType.Page:
                    prefix += "/pages";
                    break;

                default:
                    throw new InvalidOperationException("Invalid page type");
            }

            return prefix;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="HttpClient"/> class.
        /// </summary>
        /// <param name="request"><see cref="HttpRequest"/> instance.</param>
        /// <param name="handler"><see cref="HttpMessageHandler"/> instance.</param>
        /// <returns>Returns the <see cref="HttpClient"/> instance created.</returns>
        public HttpClient CreateHttpClient(HttpRequest request, HttpMessageHandler handler = null)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var client = handler == null ? new HttpClient() : new HttpClient(handler, true);
            client.BaseAddress = this.GetBaseUri(request, PublishMode.Publish);
            return client;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="HttpClient"/> class.
        /// </summary>
        /// <param name="request"><see cref="HttpRequest"/> instance.</param>
        /// <param name="mode"><see cref="PublishMode"/> value.</param>
        /// <param name="handler"><see cref="HttpMessageHandler"/> instance.</param>
        /// <returns>Returns the <see cref="HttpClient"/> instance created.</returns>
        public HttpClient CreateHttpClient(HttpRequest request, PublishMode mode, HttpMessageHandler handler = null)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var client = handler == null ? new HttpClient() : new HttpClient(handler, true);
            client.BaseAddress = this.GetBaseUri(request, mode);
            return client;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="StringContent"/> class.
        /// </summary>
        /// <param name="model">Model to be embedded.</param>
        /// <returns>Returns the <see cref="StringContent"/> instance created.</returns>
        public StringContent CreateStringContent(object model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8)
            {
                Headers =
                                      {
                                          ContentType = new MediaTypeHeaderValue(MediaType)
                                                            {
                                                                CharSet = CharSet
                                                            }
                                      }
            };
            return content;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this._disposed = true;
        }

        private static Uri GetBaseUri(HttpRequest request)
        {
            var url = string.Join("://", request.IsHttps ? "https" : "http", request.Host.Value);
            var uri = new Uri(url);
            return uri;
        }
    }
}