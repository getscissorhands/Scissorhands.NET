using System;

using Aliencube.Scissorhands.Services.Helpers;

namespace Aliencube.Scissorhands.Services.Tests.Fixtures
{
    /// <summary>
    /// This represents the fixture entity for the <see cref="FileHelper"/> class.
    /// </summary>
    public class FileHelperFixture : IDisposable
    {
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileHelperFixture"/> class.
        /// </summary>
        public FileHelperFixture()
        {
            this.FileHelper = new FileHelper();
        }

        /// <summary>
        /// Gets the <see cref="IFileHelper"/> instance.
        /// </summary>
        public IFileHelper FileHelper { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this.FileHelper.Dispose();

            this._disposed = true;
        }
    }
}