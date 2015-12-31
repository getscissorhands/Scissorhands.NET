using System;

namespace Scissorhands.Helpers.Tests.Fixtures
{
    /// <summary>
    /// This represents the fixture entity for the <see cref="HttpClientHelper"/> class.
    /// </summary>
    public class HttpClientHelperFixture : IDisposable
    {
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientHelperFixture"/> class.
        /// </summary>
        public HttpClientHelperFixture()
        {
            this.HttpClientHelper = new HttpClientHelper();
        }

        /// <summary>
        /// Gets the <see cref="HttpClientHelper"/> instance.
        /// </summary>
        public HttpClientHelper HttpClientHelper { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this.HttpClientHelper.Dispose();

            this._disposed = true;
        }
    }
}