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
    /// Add layouts to the application.
    /// </summary>
    /// <typeparam name="TMainLayout">Type of the main layout.</typeparam>
    /// <typeparam name="TIndexView">Type of the index view.</typeparam>
    /// <typeparam name="TPostView">Type of the post view.</typeparam>
    /// <typeparam name="TPageView">Type of the page view.</typeparam>
    /// <typeparam name="TNotFoundView">Type of the not found view.</typeparam>
    /// <returns>Returns <see cref="IScissorHandsApplicationBuilder"/> instance.</returns>
    IScissorHandsApplicationBuilder AddLayouts<TMainLayout, TIndexView, TPostView, TPageView, TNotFoundView>()
        where TMainLayout : MainLayoutBase
        where TIndexView : IndexViewBase
        where TPostView : PostViewBase
        where TPageView : PageViewBase
        where TNotFoundView : NotFoundViewBase;

    /// <summary>
    /// Add layouts to the application.
    /// </summary>
    /// <param name="mainLayout">Type of the main layout.</param>
    /// <param name="indexView">Type of the index view.</param>
    /// <param name="postView">Type of the post view.</param>
    /// <param name="pageView">Type of the page view.</param>
    /// <param name="notFoundView">Type of the not found view.</param>
    /// <returns>Returns <see cref="IScissorHandsApplicationBuilder"/> instance.</returns>    
    IScissorHandsApplicationBuilder AddLayouts(Type mainLayout, Type indexView, Type postView, Type pageView, Type notFoundView);

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
    private readonly string[] _args = (string[])(args ?? []);

    private Type? _mainLayout;
    private Type? _indexView;
    private Type? _postView;
    private Type? _pageView;
    private Type? _notFoundView;

    /// <inheritdoc />
    public IScissorHandsApplicationBuilder AddLayouts<TMainLayout, TIndexView, TPostView, TPageView, TNotFoundView>()
        where TMainLayout : MainLayoutBase
        where TIndexView : IndexViewBase
        where TPostView : PostViewBase
        where TPageView : PageViewBase
        where TNotFoundView : NotFoundViewBase
    {
        return AddLayouts(typeof(TMainLayout), typeof(TIndexView), typeof(TPostView), typeof(TPageView), typeof(TNotFoundView));
    }

    /// <inheritdoc />
    public IScissorHandsApplicationBuilder AddLayouts(Type mainLayout, Type indexView, Type postView, Type pageView, Type notFoundView)
    {
        ArgumentNullException.ThrowIfNull(mainLayout);
        ArgumentNullException.ThrowIfNull(indexView);
        ArgumentNullException.ThrowIfNull(postView);
        ArgumentNullException.ThrowIfNull(pageView);
        ArgumentNullException.ThrowIfNull(notFoundView);

        EnsureAssignableTo<MainLayoutBase>(mainLayout, nameof(mainLayout));
        EnsureAssignableTo<IndexViewBase>(indexView, nameof(indexView));
        EnsureAssignableTo<PostViewBase>(postView, nameof(postView));
        EnsureAssignableTo<PageViewBase>(pageView, nameof(pageView));
        EnsureAssignableTo<NotFoundViewBase>(notFoundView, nameof(notFoundView));

        _mainLayout = mainLayout;
        _indexView = indexView;
        _postView = postView;
        _pageView = pageView;
        _notFoundView = notFoundView;

        return this;
    }

    /// <inheritdoc />
    public IScissorHandsApplication Build()
    {
        if (_mainLayout is null || _indexView is null || _postView is null || _pageView is null || _notFoundView is null)
        {
            throw new InvalidOperationException("Layouts are not configured. Call AddLayouts(...) before Build().");
        }

        var builder = WebApplication.CreateBuilder(_args);

        var config = builder.Configuration;
        builder.Services
               .AddConfigurations(config)
               .AddServices(config)
               .AddRazorComponents();

        var app = builder.Build();

        return new ScissorHandsApplication(app, _args, _mainLayout, _indexView, _postView, _pageView, _notFoundView);
    }

    private static void EnsureAssignableTo<TBase>(Type type, string paramName)
    {
        if (typeof(TBase).IsAssignableFrom(type) == false)
        {
            throw new ArgumentException($"Type '{type.FullName}' must derive from '{typeof(TBase).FullName}'.", paramName);
        }
    }
}
