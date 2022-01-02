using System.Collections.Generic;
using System.Threading.Tasks;

namespace Scissorhands.Core.Abstractions
{
    /// <summary>
    /// This provides interfaces to <see cref="ContentsResolver"/> class.
    /// </summary>
    public interface IContentsResolver
    {
        /// <summary>
        /// Gets or sets the parent directory of the contents.
        /// </summary>
        string ParentDirectory { get; set; }

        /// <summary>
        /// Resolves the content items structure.
        /// </summary>
        /// <returns>Returns the list of <see cref="ContentItem"/> objects.</returns>
        Task<List<ContentItem>> ResolveAsync();
    }
}