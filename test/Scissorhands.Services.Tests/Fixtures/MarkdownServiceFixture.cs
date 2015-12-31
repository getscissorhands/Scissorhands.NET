using System;

using Moq;

using Scissorhands.Helpers;

namespace Scissorhands.Services.Tests.Fixtures
{
    /// <summary>
    /// This represents the fixture entity for the <see cref="MarkdownService"/> class.
    /// </summary>
    public class MarkdownServiceFixture : IDisposable
    {
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownServiceFixture"/> class.
        /// </summary>
        public MarkdownServiceFixture()
        {
            this.MarkdownHelper = new Mock<IMarkdownHelper>();
            this.FileHelper = new Mock<IFileHelper>();
            this.MarkdownService = new MarkdownService(this.MarkdownHelper.Object, this.FileHelper.Object);
        }

        /// <summary>
        /// Gets the <see cref="Mock{IMarkdownHelper}"/> instance.
        /// </summary>
        public Mock<IMarkdownHelper> MarkdownHelper { get; }

        /// <summary>
        /// Gets the <see cref="Mock{IFileHelper}"/> instance.
        /// </summary>
        public Mock<IFileHelper> FileHelper { get; }

        /// <summary>
        /// Gets the <see cref="IMarkdownService"/> instance.
        /// </summary>
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