using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Scissorhands.Core.Abstractions;

namespace Scissorhands.Core.Resolvers
{
    /// <summary>
    /// This represents the entity that resolves the contents structure.
    /// </summary>
    public class ContentsResolver : IContentsResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentsResolver"/> class.
        /// </summary>
        /// <param name="parentDirectory">The parent directory of the contents.</param>
        public ContentsResolver(string? parentDirectory = default)
        {
            this.ParentDirectory = parentDirectory;
        }

        /// <inheritdoc />
        public string? ParentDirectory { get; set; }

        /// <inheritdoc />
        public async Task<List<ContentItem>> ResolveAsync()
        {
            var directories = GetSubdirectories(this.ParentDirectory);
            var files = GetFiles(this.ParentDirectory);

            var contentItems = files.Select(p => new ContentItem(p.FullName)).ToList();
            foreach (var contentItem in contentItems)
            {
                await contentItem.LoadContentAsync().ConfigureAwait(false);

                if (!contentItem.IsDirectory(directories))
                {
                    continue;
                }

                var dir = new DirectoryInfo(contentItem.FilePath.Replace(Constants.MarkdownExtension, string.Empty));
                contentItem.ChildItems = await new ContentsResolver(dir.FullName).ResolveAsync().ConfigureAwait(false);
            }

            return contentItems.OrderBy(p => p.Order).ThenBy(p => p.UrlPath).ToList();
        }

        private static IEnumerable<DirectoryInfo> GetSubdirectories(string directory)
        {
            var di = new DirectoryInfo(directory);
            var directories = di.GetDirectories(Constants.AllDirectories, SearchOption.TopDirectoryOnly);

            return directories;
        }

        private static IEnumerable<FileInfo> GetFiles(string directory)
        {
            var di = new DirectoryInfo(directory);
            var files = di.GetFiles(Constants.AllMarkdownFiles, SearchOption.TopDirectoryOnly);

            return files;
        }
    }
}