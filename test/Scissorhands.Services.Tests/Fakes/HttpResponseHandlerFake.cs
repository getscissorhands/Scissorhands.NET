using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Scissorhands.Services.Tests.Fakes
{
    /// <summary>
    /// This represents the handler entity for fake HTTP response.
    /// </summary>
    /// <remarks>Reference: http://stackoverflow.com/questions/22223223/how-to-pass-in-a-mocked-httpclient-in-a-net-test#22264503</remarks>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public class HttpResponseHandlerFake : DelegatingHandler
    {
        private readonly HttpResponseMessage _message;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseHandlerFake"/> class.
        /// </summary>
        /// <param name="message"><see cref="HttpResponseMessage"/> object.</param>
        public HttpResponseHandlerFake(HttpResponseMessage message)
        {
            this._message = message;
        }

        /// <summary>
        /// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
        /// </summary>
        /// <param name="request"><see cref="HttpRequestMessage"/> instance.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> object.</param>
        /// <returns>Returns the <see cref="HttpResponseMessage"/> instance.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await Task.FromResult(this._message).ConfigureAwait(false);
        }
    }
}