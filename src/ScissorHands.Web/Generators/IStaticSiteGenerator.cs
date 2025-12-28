using ScissorHands.Theme;

namespace ScissorHands.Web.Generators;

/// <summary>
/// This provides the interface to the static site generator.
/// </summary>
public interface IStaticSiteGenerator
{
    /// <summary>
    /// Builds the static site contents.
    /// </summary>
    /// <typeparam name="TMainLayout">Type of the main layout component.</typeparam>
    /// <typeparam name="TIndexView">Type of the index view component.</typeparam>
    /// <typeparam name="TPostView">Type of the post view component.</typeparam>
    /// <typeparam name="TPageView">Type of the page view component.</typeparam>
    /// <typeparam name="TNotFoundView">Type of the not found (404) view component.</typeparam>
    /// <param name="destination">The destination directory store the generated contents.</param>
    /// <param name="preview">Indicates whether to generate a preview version.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task BuildAsync<TMainLayout, TIndexView, TPostView, TPageView, TNotFoundView>(string destination, bool preview, CancellationToken cancellationToken)
        where TMainLayout : MainLayoutBase
        where TIndexView : IndexViewBase
        where TPostView : PostViewBase
        where TPageView : PageViewBase
        where TNotFoundView : NotFoundViewBase;
}
