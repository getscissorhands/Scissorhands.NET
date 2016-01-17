using System;
using System.Net.Http;

using Microsoft.AspNet.Http;

using Scissorhands.Models.Settings;

namespace Scissorhands.Helpers
{
    /// <summary>
    /// This provides interfaces to the http request helper class.
    /// </summary>
    public interface IHttpRequestHelper : IDisposable
    {
        /// <summary>
        /// Gets the <see cref="Uri"/> instance representing base URI.
        /// </summary>
        /// <param name="request"><see cref="HttpRequest"/> instance.</param>
        /// <param name="mode"><see cref="PublishMode"/> value.</param>
        /// <returns>Returns the <see cref="Uri"/> instance representing base URI.</returns>
        Uri GetBaseUri(HttpRequest request, PublishMode mode);

        /// <summary>
        /// Gets the prefix of the slug.
        /// </summary>
        /// <param name="request"><see cref="HttpRequest"/> instance.</param>
        /// <returns>Returns the prefix of the slug.</returns>
        string GetSlugPrefix(HttpRequest request);

        /// <summary>
        /// Gets the prefix of the slug.
        /// </summary>
        /// <param name="request"><see cref="HttpRequest"/> instance.</param>
        /// <param name="mode"><see cref="PublishMode"/> value.</param>
        /// <returns>Returns the prefix of the slug.</returns>
        string GetSlugPrefix(HttpRequest request, PublishMode mode);

        /// <summary>
        /// Creates a new instance of the <see cref="HttpClient"/> class.
        /// </summary>
        /// <param name="request"><see cref="HttpRequest"/> instance.</param>
        /// <param name="handler"><see cref="HttpMessageHandler"/> instance.</param>
        /// <returns>Returns the <see cref="HttpClient"/> instance created.</returns>
        HttpClient CreateHttpClient(HttpRequest request, HttpMessageHandler handler = null);

        /// <summary>
        /// Creates a new instance of the <see cref="HttpClient"/> class.
        /// </summary>
        /// <param name="request"><see cref="HttpRequest"/> instance.</param>
        /// <param name="mode"><see cref="PublishMode"/> value.</param>
        /// <param name="handler"><see cref="HttpMessageHandler"/> instance.</param>
        /// <returns>Returns the <see cref="HttpClient"/> instance created.</returns>
        HttpClient CreateHttpClient(HttpRequest request, PublishMode mode, HttpMessageHandler handler = null);

        /// <summary>
        /// Creates a new instance of the <see cref="StringContent"/> class.
        /// </summary>
        /// <param name="model">Model to be embedded.</param>
        /// <returns>Returns the <see cref="StringContent"/> instance created.</returns>
        StringContent CreateStringContent(object model);
    }
}