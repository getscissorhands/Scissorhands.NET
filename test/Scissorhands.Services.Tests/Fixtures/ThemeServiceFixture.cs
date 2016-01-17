using System;

using Moq;

using Scissorhands.Models.Settings;

namespace Scissorhands.Services.Tests.Fixtures
{
    /// <summary>
    /// This represents the fixture entity for the <see cref="ThemeService"/> class.
    /// </summary>
    public class ThemeServiceFixture : IDisposable
    {
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeServiceFixture"/> class.
        /// </summary>
        public ThemeServiceFixture()
        {
            this.DefaultThemeName = "default";

            this.SiteMetadataSettings = new Mock<SiteMetadataSettings>();
            this.SiteMetadataSettings.SetupGet(p => p.Theme).Returns(this.DefaultThemeName);

            this.ThemeService = new ThemeService(this.SiteMetadataSettings.Object);
        }

        /// <summary>
        /// Gets or sets the default theme name.
        /// </summary>
        public string DefaultThemeName { get; set; }

        /// <summary>
        /// Gets the <see cref="Mock{SiteMetadataSettings}"/> instance.
        /// </summary>
        public Mock<SiteMetadataSettings> SiteMetadataSettings { get; }

        /// <summary>
        /// Gets the <see cref="ThemeService"/> instance.
        /// </summary>
        public IThemeService ThemeService { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this.ThemeService.Dispose();

            this._disposed = true;
        }
    }
}