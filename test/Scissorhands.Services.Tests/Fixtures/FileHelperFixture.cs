using System;

using Moq;

using Scissorhands.Helpers;
using Scissorhands.Models.Settings;

namespace Scissorhands.Services.Tests.Fixtures
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
            this.WebAppSettings = new Mock<WebAppSettings>();

            this.FileHelper = new FileHelper(this.WebAppSettings.Object);
        }

        /// <summary>
        /// Gets the <see cref="Mock{WebAppSettings}"/> instance.
        /// </summary>
        public Mock<WebAppSettings> WebAppSettings { get; }

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