using System;
using System.IO;
using Microsoft.Extensions.PlatformAbstractions;
using Moq;

using Scissorhands.Models.Settings;

namespace Scissorhands.Helpers.Tests.Fixtures
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
            this.ApplicationEnvironment = new Mock<IApplicationEnvironment>();

            this.ApplicationBasePath = $"{Path.GetTempPath()}/home/scissorhands.net";
            this.ApplicationEnvironment.SetupGet(p => p.ApplicationBasePath).Returns(this.ApplicationBasePath);

            this.FileHelper = new FileHelper(this.WebAppSettings.Object, this.ApplicationEnvironment.Object);
        }
        
        /// <summary>
        /// Gets the appliation base path
        /// </summary>
        public String ApplicationBasePath { get; } 

        /// <summary>
        /// Gets the <see cref="Mock{WebAppSettings}"/> instance.
        /// </summary>
        public Mock<WebAppSettings> WebAppSettings { get; }

        /// <summary>
        /// Gets the <see cref="Mock{ApplicationEnvironment}"/> instance.
        /// </summary>
        public Mock<IApplicationEnvironment> ApplicationEnvironment { get; }

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