using System;

using Moq;

using Scissorhands.Helpers;
using Scissorhands.Models.Settings;

namespace Scissorhands.Services.Tests.Fixtures
{
    /// <summary>
    /// This represents the fixture entity for the <see cref="PublishService"/> class.
    /// </summary>
    public class PublishServiceFixture : IDisposable
    {
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishServiceFixture"/> class.
        /// </summary>
        public PublishServiceFixture()
        {
            this.DefaultThemeName = "default";

            this.WebAppSettings = new Mock<WebAppSettings>();
            this.WebAppSettings.SetupGet(p => p.MarkdownPath).Returns("/posts");
            this.WebAppSettings.SetupGet(p => p.HtmlPath).Returns("/posts");

            this.SiteMetadataSettings = new Mock<SiteMetadataSettings>();
            this.SiteMetadataSettings.SetupGet(p => p.Theme).Returns(this.DefaultThemeName);

            this.MarkdownHelper = new Mock<IMarkdownHelper>();

            this.FileHelper = new Mock<IFileHelper>();

            this.HttpClientHelper = new Mock<IHttpClientHelper>();

            this.PublishService = new PublishService(this.WebAppSettings.Object, this.SiteMetadataSettings.Object, this.MarkdownHelper.Object, this.FileHelper.Object, this.HttpClientHelper.Object);
        }

        /// <summary>
        /// Gets or sets the default theme name.
        /// </summary>
        public string DefaultThemeName { get; set; }

        /// <summary>
        /// Gets the <see cref="Mock{WebAppSettings}"/> instance..
        /// </summary>
        public Mock<WebAppSettings> WebAppSettings { get; }

        /// <summary>
        /// Gets the <see cref="Mock{SiteMetadataSettings}"/> instance.
        /// </summary>
        public Mock<SiteMetadataSettings> SiteMetadataSettings { get; }

        /// <summary>
        /// Gets the <see cref="Mock{IMarkdownHelper}"/> instance.
        /// </summary>
        public Mock<IMarkdownHelper> MarkdownHelper { get; }

        /// <summary>
        /// Gets the <see cref="Mock{IFileHelper}"/> instance.
        /// </summary>
        public Mock<IFileHelper> FileHelper { get; }

        /// <summary>
        /// Gets the <see cref="Mock{IHttpClientHelper}"/> instance.
        /// </summary>
        public Mock<IHttpClientHelper> HttpClientHelper { get; }

        /// <summary>
        /// Gets the <see cref="IPublishService"/> instance.
        /// </summary>
        public IPublishService PublishService { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this.PublishService.Dispose();

            this._disposed = true;
        }
    }
}