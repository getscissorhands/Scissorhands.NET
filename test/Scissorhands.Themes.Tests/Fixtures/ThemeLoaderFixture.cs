using System;

using Microsoft.Extensions.PlatformAbstractions;

using Moq;

using Scissorhands.Helpers;
using Scissorhands.Models.Settings;

namespace Scissorhands.Themes.Tests.Fixtures
{
    /// <summary>
    /// This represents the fixture entity for the <see cref="ThemeLoader"/> class.
    /// </summary>
    public class ThemeLoaderFixture : IDisposable
    {
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeLoaderFixture"/> class.
        /// </summary>
        public ThemeLoaderFixture()
        {
            this.DefaultThemeName = "default";

            this.WebAppSettings = new Mock<WebAppSettings>();
            this.WebAppSettings.SetupGet(p => p.Theme).Returns(this.DefaultThemeName);

            this.FileHelper = new Mock<IFileHelper>();

            this.ApplicationEnvironment = new Mock<IApplicationEnvironment>();

            this.ThemeLoader = new ThemeLoader(this.WebAppSettings.Object, this.FileHelper.Object);
        }

        /// <summary>
        /// Gets or sets the default theme name.
        /// </summary>
        public string DefaultThemeName { get; set; }

        /// <summary>
        /// Gets the <see cref="Mock{WebAppSettings}"/> instance.
        /// </summary>
        public Mock<WebAppSettings> WebAppSettings { get; }

        /// <summary>
        /// Gets the <see cref="Mock{IFileHelper}"/> instance.
        /// </summary>
        public Mock<IFileHelper> FileHelper { get; }

        /// <summary>
        /// Gets the <see cref="Mock{IApplicationEnvironment}"/> instance.
        /// </summary>
        public Mock<IApplicationEnvironment> ApplicationEnvironment { get; }

        /// <summary>
        /// Gets the <see cref="IThemeLoader"/> instance.
        /// </summary>
        public IThemeLoader ThemeLoader { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this._disposed = true;
        }
    }
}