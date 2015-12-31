using System;

using Aliencube.Scissorhands.Services.Helpers;

namespace Aliencube.Scissorhands.Services.Tests.Fixtures
{
    /// <summary>
    /// This represents the fixture entity for the <see cref="MarkdownHelper"/> class.
    /// </summary>
    public class MarkdownHelperFixture : IDisposable
    {
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownHelperFixture"/> class.
        /// </summary>
        public MarkdownHelperFixture()
        {
            this.MarkdownHelper = new MarkdownHelper();
        }

        /// <summary>
        /// Gets the <see cref="IMarkdownHelper"/> instance.
        /// </summary>
        public IMarkdownHelper MarkdownHelper { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this.MarkdownHelper.Dispose();

            this._disposed = true;
        }
    }
}