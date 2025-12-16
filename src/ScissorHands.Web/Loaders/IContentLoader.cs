using ScissorHands.Core.Models;

namespace ScissorHands.Web.Loaders;

/// <summary>
/// This provides the interface for content loader.
/// </summary>
public interface IContentLoader
{
    /// <summary>
    /// Loads the content documents.
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> value.</param>
    /// <returns>Returns the collection of <see cref="ContentDocument"/> instances.</returns>
    Task<IEnumerable<ContentDocument>> LoadAsync(CancellationToken cancellationToken);
}
