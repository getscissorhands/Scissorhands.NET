using System;
using System.Collections.Generic;

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
            this.BaseUrl = "http://localhost:5080";
            this.BasePath = "/blog";
            this.Authors = new List<Author>() { new Author() { Name = "Joe Bloggs", IsDefault = true } };
            this.FeedTypes = new List<FeedType>() { FeedType.Rss };

            this.SiteMetadataSettings = new Mock<ISiteMetadataSettings>();
            this.SiteMetadataSettings.SetupGet(p => p.Theme).Returns(this.DefaultThemeName);
            this.SiteMetadataSettings.SetupGet(p => p.BaseUrl).Returns(this.BaseUrl);
            this.SiteMetadataSettings.SetupGet(p => p.BasePath).Returns(this.BasePath);
            this.SiteMetadataSettings.SetupGet(p => p.Authors).Returns(this.Authors);
            this.SiteMetadataSettings.SetupGet(p => p.FeedTypes).Returns(this.FeedTypes);

            this.FileHelper = new Mock<IFileHelper>();

            this.ApplicationEnvironment = new Mock<IApplicationEnvironment>();

            this.ThemeLoader = new ThemeLoader(this.SiteMetadataSettings.Object, this.FileHelper.Object);
        }

        /// <summary>
        /// Gets the default theme name.
        /// </summary>
        public string DefaultThemeName { get; }

        /// <summary>
        /// Gets the base url.
        /// </summary>
        public string BaseUrl { get; }

        /// <summary>
        /// Gets the base path.
        /// </summary>
        public string BasePath { get; }

        /// <summary>
        /// Gets the list of authors.
        /// </summary>
        public List<Author> Authors { get; }

        /// <summary>
        /// Gets the list of feed types.
        /// </summary>
        public List<FeedType> FeedTypes { get; }

        /// <summary>
        /// Gets the <see cref="Mock{ISiteMetadataSettings}"/> instance.
        /// </summary>
        public Mock<ISiteMetadataSettings> SiteMetadataSettings { get; }

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