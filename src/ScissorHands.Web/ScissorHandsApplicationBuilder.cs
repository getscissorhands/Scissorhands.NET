using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using ScissorHands.Theme;
using ScissorHands.Web.Extensions;

namespace ScissorHands.Web;

/// <summary>
/// This provides the interface for ScissorHands application builder.
/// </summary>
public interface IScissorHandsApplicationBuilder
{
    /// <summary>
    /// Adds an explicit theme component override to the application.
    /// Theme components are discovered automatically when this method is not called.
    /// </summary>
    /// <typeparam name="TMainLayout">Type of the main layout.</typeparam>
    /// <typeparam name="TIndexView">Type of the index view.</typeparam>
    /// <typeparam name="TPostView">Type of the post view.</typeparam>
    /// <typeparam name="TPageView">Type of the page view.</typeparam>
    /// <typeparam name="TNotFoundView">Type of the not found view.</typeparam>
    /// <typeparam name="TTagListView">Type of the tag list view.</typeparam>
    /// <typeparam name="TTagView">Type of the individual tag view.</typeparam>
    /// <returns>Returns <see cref="IScissorHandsApplicationBuilder"/> instance.</returns>
    IScissorHandsApplicationBuilder AddLayouts<TMainLayout, TIndexView, TPostView, TPageView, TNotFoundView, TTagListView, TTagView>()
        where TMainLayout : MainLayoutBase
        where TIndexView : IndexViewBase
        where TPostView : PostViewBase
        where TPageView : PageViewBase
        where TNotFoundView : NotFoundViewBase
        where TTagListView : TagListViewBase
        where TTagView : TagViewBase;

    /// <summary>
    /// Adds an explicit theme component override to the application.
    /// Theme components are discovered automatically when this method is not called.
    /// </summary>
    /// <param name="mainLayout">Type of the main layout.</param>
    /// <param name="indexView">Type of the index view.</param>
    /// <param name="postView">Type of the post view.</param>
    /// <param name="pageView">Type of the page view.</param>
    /// <param name="notFoundView">Type of the not found view.</param>
    /// <param name="tagListView">Type of the tag list view.</param>
    /// <param name="tagView">Type of the individual tag view.</param>
    /// <returns>Returns <see cref="IScissorHandsApplicationBuilder"/> instance.</returns>    
    IScissorHandsApplicationBuilder AddLayouts(Type mainLayout, Type indexView, Type postView, Type pageView, Type notFoundView, Type tagListView, Type tagView);

    /// <summary>
    /// Builds the application.
    /// </summary>
    /// <returns>Returns <see cref="IScissorHandsApplication"/> instance.</returns>
    IScissorHandsApplication Build();
}

/// <summary>
/// This represents the builder entity for <see cref="ScissorHandsApplication"/>.
/// </summary>
public sealed class ScissorHandsApplicationBuilder(IEnumerable<string>? args = null) : IScissorHandsApplicationBuilder
{
    private readonly string[] _args = args?.ToArray() ?? [];

    private ThemeComponentSet? _themeComponents;

    /// <inheritdoc />
    public IScissorHandsApplicationBuilder AddLayouts<TMainLayout, TIndexView, TPostView, TPageView, TNotFoundView, TTagListView, TTagView>()
        where TMainLayout : MainLayoutBase
        where TIndexView : IndexViewBase
        where TPostView : PostViewBase
        where TPageView : PageViewBase
        where TNotFoundView : NotFoundViewBase
        where TTagListView : TagListViewBase
        where TTagView : TagViewBase
    {
        return AddLayouts(typeof(TMainLayout), typeof(TIndexView), typeof(TPostView), typeof(TPageView), typeof(TNotFoundView), typeof(TTagListView), typeof(TTagView));
    }

    /// <inheritdoc />
    public IScissorHandsApplicationBuilder AddLayouts(Type mainLayout, Type indexView, Type postView, Type pageView, Type notFoundView, Type tagListView, Type tagView)
    {
        ArgumentNullException.ThrowIfNull(mainLayout);
        ArgumentNullException.ThrowIfNull(indexView);
        ArgumentNullException.ThrowIfNull(postView);
        ArgumentNullException.ThrowIfNull(pageView);
        ArgumentNullException.ThrowIfNull(notFoundView);
        ArgumentNullException.ThrowIfNull(tagListView);
        ArgumentNullException.ThrowIfNull(tagView);

        EnsureAssignableTo<MainLayoutBase>(mainLayout, nameof(mainLayout));
        EnsureAssignableTo<IndexViewBase>(indexView, nameof(indexView));
        EnsureAssignableTo<PostViewBase>(postView, nameof(postView));
        EnsureAssignableTo<PageViewBase>(pageView, nameof(pageView));
        EnsureAssignableTo<NotFoundViewBase>(notFoundView, nameof(notFoundView));
        EnsureAssignableTo<TagListViewBase>(tagListView, nameof(tagListView));
        EnsureAssignableTo<TagViewBase>(tagView, nameof(tagView));

        _themeComponents = new ThemeComponentSet(mainLayout, indexView, postView, pageView, notFoundView, tagListView, tagView);

        return this;
    }

    /// <inheritdoc />
    public IScissorHandsApplication Build()
    {
        var builder = WebApplication.CreateBuilder(_args);

        var config = builder.Configuration;
        builder.Services
               .AddConfigurations(config)
               .AddServices(config)
               .AddRazorComponents();

        var app = builder.Build();

        return new ScissorHandsApplication(app, _args, _themeComponents);
    }

    private static void EnsureAssignableTo<TBase>(Type type, string paramName)
    {
        if (typeof(TBase).IsAssignableFrom(type) == false)
        {
            throw new ArgumentException($"Type '{type.FullName}' must derive from '{typeof(TBase).FullName}'.", paramName);
        }
    }
}
