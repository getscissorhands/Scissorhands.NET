using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

using Microsoft.AspNet.Http;

using Newtonsoft.Json;

namespace Scissorhands.Helpers
{
    /// <summary>
    /// This represents the helper entity for <see cref="HttpClient"/> and its related ones.
    /// </summary>
    public class HttpClientHelper : IHttpClientHelper
    {
        private const string MediaType = "application/json";
        private const string CharSet = "utf-8";

        private bool _disposed;

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

            var url = string.Join("://", request.IsHttps ? "https" : "http", request.Host.Value);
            var baseAddressUri = new Uri(url);

            var client = handler == null ? new HttpClient() : new HttpClient(handler, true);
            client.BaseAddress = baseAddressUri;
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
    }
}