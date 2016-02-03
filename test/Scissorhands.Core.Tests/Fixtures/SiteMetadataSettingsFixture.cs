using System;

using Scissorhands.Models.Settings;

namespace Scissorhands.Core.Tests.Fixtures
{
    /// <summary>
    /// This represents the fixture entity for the <see cref="SiteMetadataSettingsTest"/> class.
    /// </summary>
    public class SiteMetadataSettingsFixture : IDisposable
    {
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteMetadataSettingsFixture"/> class.
        /// </summary>
        public SiteMetadataSettingsFixture()
        {
            this.SiteMetadataSettings = new SiteMetadataSettings();
        }

        /// <summary>
        /// Gets the <see cref="ISiteMetadataSettings"/> instance.
        /// </summary>
        public ISiteMetadataSettings SiteMetadataSettings { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this.SiteMetadataSettings.Dispose();

            this._disposed = true;
        }
    }
}