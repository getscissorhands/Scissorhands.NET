using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNet.Http;

using Scissorhands.Helpers;
using Scissorhands.Models.Posts;
using Scissorhands.Models.Settings;
using Scissorhands.Services.Exceptions;
using Scissorhands.ViewModels.Post;

namespace Scissorhands.Services
{
    /// <summary>
    /// This represents the service entity for publish.
    /// </summary>
    public class PublishService : IPublishService
    {
        private const string PostPublishHtml = "/admin/post/publish/html";

        private readonly WebAppSettings _settings;
        private readonly SiteMetadataSettings _metadata;
        private readonly IFileHelper _fileHelper;
        private readonly IHttpRequestHelper _requestHelper;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishService"/> class.
        /// </summary>
        /// <param name="settings"><see cref="WebAppSettings"/> instance.</param>
        /// <param name="metadata"><see cref="SiteMetadataSettings"/> instance.</param>
        /// <param name="fileHelper"><see cref="IFileHelper"/> instance.</param>
        /// <param name="requestHelper"><see cref="IHttpRequestHelper"/> instance.</param>
        public PublishService(WebAppSettings settings, SiteMetadataSettings metadata, IFileHelper fileHelper, IHttpRequestHelper requestHelper)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            this._settings = settings;

            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            this._metadata = metadata;

            if (fileHelper == null)
            {
                throw new ArgumentNullException(nameof(fileHelper));
            }

            this._fileHelper = fileHelper;

            if (requestHelper == null)
            {
                throw new ArgumentNullException(nameof(requestHelper));
            }

            this._requestHelper = requestHelper;
        }

        /// <summary>
        /// Applies metadata to the markdown body.
        /// </summary>
        /// <param name="model"><see cref="PostFormViewModel"/> instance.</param>
        /// <returns>Returns the markdown body with metadata applied.</returns>
        public string ApplyMetadata(PostFormViewModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var metadata = new PublishedMetadata()
                               {
                                   Title = model.Title,
                                   Slug = model.Slug,
                                   Author = model.Author,
                                   DatePublished = DateTime.Now,
                                   Tags = GetTags(model.Tags),
                               };

            var sb = new StringBuilder();
            sb.AppendLine("---");
            sb.AppendLine($"* Title: {metadata.Title}");
            sb.AppendLine($"* Slug: {metadata.Slug}");
            sb.AppendLine($"* Author: {metadata.Author}");
            sb.AppendLine($"* Date Published: {metadata.DatePublished.ToString(this._metadata.DateTimeFormat)}");
            sb.AppendLine($"* Tags: {string.Join(", ", metadata.Tags)}");
            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine(model.Body);

            return sb.ToString();
        }

        /// <summary>
        /// Publishes the markdown as a file.
        /// </summary>
        /// <param name="markdown">Content in Markdown format.</param>
        /// <returns>Returns the Markdown file path in a virtual path format.</returns>
        public async Task<string> PublishMarkdownAsync(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
            {
                throw new ArgumentNullException(nameof(markdown));
            }

            var filename = "markdown.md";
            var markdownpath = $"{this._settings.MarkdownPath}/{filename}";

            var filepath = this._fileHelper.GetDirectory(this._settings.MarkdownPath);
            filepath = Path.Combine(new[] { filepath, filename });

            var written = await this._fileHelper.WriteAsync(filepath, markdown).ConfigureAwait(false);
            if (!written)
            {
                throw new PublishFailedException("Markdown not published");
            }

            return markdownpath;
        }

        /// <summary>
        /// Publishes the HTML post as a file.
        /// </summary>
        /// <param name="html">Content in HTML format.</param>
        /// <returns>Returns the HTML file path.</returns>
        public async Task<string> PublishHtmlAsync(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                throw new ArgumentNullException(nameof(html));
            }

            var filename = "post.html";
            var htmlpath = $"{this._settings.HtmlPath}/{filename}";

            var filepath = this._fileHelper.GetDirectory(this._settings.HtmlPath);
            filepath = Path.Combine(new[] { filepath, filename });

            var written = await this._fileHelper.WriteAsync(filepath, html).ConfigureAwait(false);
            if (!written)
            {
                throw new PublishFailedException("Post not published");
            }

            return htmlpath;
        }

        /// <summary>
        /// Gets the published HTML content.
        /// </summary>
        /// <param name="model"><see cref="PostFormViewModel"/> instance.</param>
        /// <param name="request"><see cref="HttpRequest"/> instance.</param>
        /// <returns>Returns the published HTML content.</returns>
        public async Task<string> GetPublishedHtmlAsync(PostFormViewModel model, HttpRequest request)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            using (var client = this._requestHelper.CreateHttpClient(request, PublishMode.Parse))
            using (var content = this._requestHelper.CreateStringContent(model))
            {
                var response = await client.PostAsync(PostPublishHtml, content).ConfigureAwait(false);
                var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return html;
            }
        }

        /// <summary>
        /// Publishes the post as a file.
        /// </summary>
        /// <param name="model"><see cref="PostFormViewModel"/> instance.</param>
        /// <param name="request"><see cref="HttpRequest"/> instance.</param>
        /// <returns>Returns the <see cref="PublishedPostPath"/> instance containing paths for published files.</returns>
        public async Task<PublishedPostPath> PublishPostAsync(PostFormViewModel model, HttpRequest request)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var publishedpath = new PublishedPostPath();

            var markdown = this.ApplyMetadata(model);

            var markdownpath = await this.PublishMarkdownAsync(markdown).ConfigureAwait(false);
            publishedpath.Markdown = markdownpath;

            var html = await this.GetPublishedHtmlAsync(model, request).ConfigureAwait(false);

            var htmlpath = await this.PublishHtmlAsync(html).ConfigureAwait(false);
            publishedpath.Html = htmlpath;

            return publishedpath;
        }

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

        private static List<string> GetTags(string tags)
        {
            if (string.IsNullOrWhiteSpace(tags))
            {
                return null;
            }

            var list = tags.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToList();
            return list;
        }
    }
}