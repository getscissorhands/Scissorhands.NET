using System;
using System.Collections.Generic;
using System.Linq;

using Moq;

using Scissorhands.Helpers;
using Scissorhands.Models.Posts;
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

            this.SiteMetadataSettings = new Mock<ISiteMetadataSettings>();
            this.SiteMetadataSettings.SetupGet(p => p.Theme).Returns(this.DefaultThemeName);

            this.FileHelper = new Mock<IFileHelper>();

            this.HttpRequestHelper = new Mock<IHttpRequestHelper>();

            this.PublishService = new PublishService(this.WebAppSettings.Object, this.SiteMetadataSettings.Object, this.FileHelper.Object, this.HttpRequestHelper.Object);

            this.Title = "Hello World";
            this.Slug = "hello-world";
            this.Author = "Joe Bloggs";
            this.DatePublished = DateTime.Now;
            this.Tags = new[] { "hello", "world" }.ToList();

            this.PublishedMetadata = new Mock<PublishedMetadata>();
            this.PublishedMetadata.SetupGet(p => p.Title).Returns(this.Title);
            this.PublishedMetadata.SetupGet(p => p.Slug).Returns(this.Slug);
            this.PublishedMetadata.SetupGet(p => p.Author).Returns(this.Author);
            this.PublishedMetadata.SetupGet(p => p.DatePublished).Returns(this.DatePublished);
            this.PublishedMetadata.SetupGet(p => p.Tags).Returns(this.Tags);
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
        /// Gets the <see cref="Mock{ISiteMetadataSettings}"/> instance.
        /// </summary>
        public Mock<ISiteMetadataSettings> SiteMetadataSettings { get; }

        /// <summary>
        /// Gets the <see cref="Mock{IFileHelper}"/> instance.
        /// </summary>
        public Mock<IFileHelper> FileHelper { get; }

        /// <summary>
        /// Gets the <see cref="Mock{IHttpRequestHelper}"/> instance.
        /// </summary>
        public Mock<IHttpRequestHelper> HttpRequestHelper { get; }

        /// <summary>
        /// Gets the <see cref="IPublishService"/> instance.
        /// </summary>
        public IPublishService PublishService { get; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the slug.
        /// </summary>
        public string Slug { get; set; }

        /// <summary>
        /// Gets or sets the author.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets the date published.
        /// </summary>
        public DateTime DatePublished { get; set; }

        /// <summary>
        /// Gets or sets the list of tags.
        /// </summary>
        public List<string> Tags { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Mock{PublishedMetadata}"/> instance.
        /// </summary>
        public Mock<PublishedMetadata> PublishedMetadata { get; set; }

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