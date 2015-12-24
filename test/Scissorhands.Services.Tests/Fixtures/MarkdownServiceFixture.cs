using System;

using Aliencube.Scissorhands.Services.Helpers;

using Moq;

namespace Aliencube.Scissorhands.Services.Tests.Fixtures
{
    /// <summary>
    /// This represents the fixture entity for the <see cref="MarkdownService"/> class.
    /// </summary>
    public class MarkdownServiceFixture : IDisposable
    {
        private readonly Mock<IFileHelper> _helper;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownServiceFixture"/> class.
        /// </summary>
        public MarkdownServiceFixture()
        {
            this._helper = new Mock<IFileHelper>();
            this._helper.Setup(p => p.GetName(It.IsAny<string>())).Returns("Hello World");

            this.MarkdownService = new MarkdownService(this._helper.Object);
        }

        /// <summary>
        /// Gets the markdown service.
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