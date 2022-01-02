using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Scissorhands.Core.Extensions;

namespace Scissorhands.Core
{
    /// <summary>
    /// This represents the entity of content item.
    /// </summary>
    public class ContentItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentItem"/> class.
        /// </summary>
        public ContentItem(string filepath)
        {
            this.FilePath = filepath ?? throw new ArgumentNullException(nameof(filepath));

            this.ChildItems = new List<ContentItem>();
        }

        /// <summary>
        /// Gets the content item path relative to the content root.
        /// </summary>
        public virtual string? FilePath { get; private set; }

        /// <summary>
        /// Gets the URL path of the content.
        /// </summary>
        public virtual string? UrlPath { get; private set; }

        /// <summary>
        /// Gets the <see cref="FrontMatter"/> object.
        /// </summary>
        public virtual FrontMatter FrontMatter { get; private set; }

        /// <summary>
        /// Gets the body as markdown.
        /// </summary>
        public virtual string? MarkdownBody { get; private set; }

        /// <summary>
        /// Gets the body as HTML.
        /// </summary>
        public virtual string? HtmlBody { get; private set; }

        /// <summary>
        /// Gets the raw content body.
        /// </summary>
        public virtual string? RawBody { get; private set; }

        /// <summary>
        /// Gets the content order.
        /// </summary>
        public virtual int? Order { get; private set; }

        /// <summary>
        /// Gets or sets the list of <see cref="ContentItem"/> objects as its children.
        /// </summary>
        public virtual List<ContentItem> ChildItems { get; set; }

        /// <summary>
        /// Gets the value indicating whether the content item is a directory or not.
        /// </summary>
        public virtual bool IsDirectory(IEnumerable<DirectoryInfo> directories)
        {
            var isDirectory = directories.Select(q => q.FullName).Contains(this.FilePath.Replace(".md", ""));

            return isDirectory;
        }

        /// <summary>
        /// Loads the content from file.
        /// </summary>
        public virtual async Task LoadContentAsync()
        {
            using (var stream = new FileStream(this.FilePath, FileMode.Open))
            using (var reader = new StreamReader(stream))
            {
                this.RawBody = await reader.ReadToEndAsync();
            }

            var segments = this.RawBody.Split(new[] { "---" }, StringSplitOptions.RemoveEmptyEntries);
            this.FrontMatter = segments.First().Trim().ToFrontMatter();
            this.MarkdownBody = string.Join("\r\n---\r\n", segments.Skip(1).Select(p => p.Trim()));
            this.HtmlBody = this.MarkdownBody.ToHtml();
            this.UrlPath = GetUrlPath(this.FilePath, this.FrontMatter?.Slug);
            this.Order = this.FrontMatter?.Order ?? 0;
        }

        private static string GetUrlPath(string filepath, string slug)
        {
            var fi = new FileInfo(filepath);
            var paths = fi.FullName.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var skip = paths.IndexOf(Constants.Contents);

            var urlpath = $"/{string.Join("/", paths.Skip(skip + 1)).Replace(Constants.MarkdownExtension, string.Empty).ToLowerInvariant()}";
            var result = string.IsNullOrWhiteSpace(slug)
                         ? $"/{urlpath.TrimStart('/')}"
                         : $"/{slug.TrimStart('/')}";

            if (result == "/index")
            {
                result = "/";
            }

            return result;
        }
    }
}