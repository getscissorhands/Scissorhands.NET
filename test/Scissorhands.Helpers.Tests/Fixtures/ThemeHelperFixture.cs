using System;

using Moq;

using Scissorhands.Models.Settings;

namespace Scissorhands.Helpers.Tests.Fixtures
{
    /// <summary>
    /// This represents the fixture entity for the <see cref="ThemeHelperTest"/> class.
    /// </summary>
    public class ThemeHelperFixture : IDisposable
    {
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeHelperFixture"/> class.
        /// </summary>
        public ThemeHelperFixture()
        {
            this.DefaultThemeName = "default";

            this.SiteMetadataSettings = new Mock<ISiteMetadataSettings>();
            this.SiteMetadataSettings.SetupGet(p => p.Theme).Returns(this.DefaultThemeName);

            this.ThemeHelper = new ThemeHelper(this.SiteMetadataSettings.Object);
        }

        /// <summary>
        /// Gets or sets the default theme name.
        /// </summary>
        public string DefaultThemeName { get; set; }

        /// <summary>
        /// Gets the <see cref="Mock{ISiteMetadataSettings}"/> instance.
        /// </summary>
        public Mock<ISiteMetadataSettings> SiteMetadataSettings { get; }

        /// <summary>
        /// Gets the <see cref="IThemeHelper"/> instance.
        /// </summary>
        public IThemeHelper ThemeHelper { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this.ThemeHelper.Dispose();

            this._disposed = true;
        }
    }
}