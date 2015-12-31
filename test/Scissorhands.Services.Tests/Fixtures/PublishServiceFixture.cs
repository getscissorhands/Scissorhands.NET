using System;

using Aliencube.Scissorhands.Models;
using Aliencube.Scissorhands.Services.Helpers;

using Moq;

namespace Aliencube.Scissorhands.Services.Tests.Fixtures
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
            this.WebAppSettings.SetupGet(p => p.Theme).Returns(this.DefaultThemeName);
            this.WebAppSettings.SetupGet(p => p.MarkdownPath).Returns("/posts");
            this.WebAppSettings.SetupGet(p => p.HtmlPath).Returns("/posts");

            this.FileHelper = new Mock<IFileHelper>();

            this.PublishService = new PublishService(this.WebAppSettings.Object, this.FileHelper.Object);
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
        /// Gets the <see cref="Mock{IFileHelper}"/> instance.
        /// </summary>
        public Mock<IFileHelper> FileHelper { get; }

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