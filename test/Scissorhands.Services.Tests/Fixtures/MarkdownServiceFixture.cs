using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aliencube.Scissorhands.Services.Tests.Fixtures
{
    public class MarkdownServiceFixture : IDisposable
    {
        private bool _disposed;

        public MarkdownServiceFixture()
        {
            this.MarkdownService = new MarkdownService();
        }

        public IMarkdownService MarkdownService { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this.MarkdownService.Dispose();

            this._disposed = true;
        }
    }
}
