using System;
using System.Net.Http;

using Microsoft.AspNet.Http;

namespace Scissorhands.Helpers
{
    /// <summary>
    /// This provides interfaces to the http client helper class.
    /// </summary>
    public interface IHttpClientHelper : IDisposable
    {
        /// <summary>
        /// Creates a new instance of the <see cref="HttpClient"/> class.
        /// </summary>
        /// <param name="request"><see cref="HttpRequest"/> instance.</param>
        /// <param name="handler"><see cref="HttpMessageHandler"/> instance.</param>
        /// <returns>Returns the <see cref="HttpClient"/> instance created.</returns>
        HttpClient CreateHttpClient(HttpRequest request, HttpMessageHandler handler = null);

        /// <summary>
        /// Creates a new instance of the <see cref="StringContent"/> class.
        /// </summary>
        /// <param name="model">Model to be embedded.</param>
        /// <returns>Returns the <see cref="StringContent"/> instance created.</returns>
        StringContent CreateStringContent(object model);
    }
}