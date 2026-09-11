namespace ScissorHands.Theme;

/// <summary>
/// Defines the Razor components that make up a theme.
/// </summary>
public sealed class ThemeComponentSet
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeComponentSet"/> class.
    /// </summary>
    public ThemeComponentSet(
        Type mainLayout,
        Type indexView,
        Type postView,
        Type pageView,
        Type notFoundView,
        Type tagListView,
        Type tagView)
    {
        MainLayout = EnsureAssignableTo<MainLayoutBase>(mainLayout, nameof(mainLayout));
        IndexView = EnsureAssignableTo<IndexViewBase>(indexView, nameof(indexView));
        PostView = EnsureAssignableTo<PostViewBase>(postView, nameof(postView));
        PageView = EnsureAssignableTo<PageViewBase>(pageView, nameof(pageView));
        NotFoundView = EnsureAssignableTo<NotFoundViewBase>(notFoundView, nameof(notFoundView));
        TagListView = EnsureAssignableTo<TagListViewBase>(tagListView, nameof(tagListView));
        TagView = EnsureAssignableTo<TagViewBase>(tagView, nameof(tagView));
    }

    /// <summary>
    /// Gets the main layout component.
    /// </summary>
    public Type MainLayout { get; }

    /// <summary>
    /// Gets the index view component.
    /// </summary>
    public Type IndexView { get; }

    /// <summary>
    /// Gets the post view component.
    /// </summary>
    public Type PostView { get; }

    /// <summary>
    /// Gets the page view component.
    /// </summary>
    public Type PageView { get; }

    /// <summary>
    /// Gets the not-found view component.
    /// </summary>
    public Type NotFoundView { get; }

    /// <summary>
    /// Gets the tag-list view component.
    /// </summary>
    public Type TagListView { get; }

    /// <summary>
    /// Gets the tag view component.
    /// </summary>
    public Type TagView { get; }

    private static Type EnsureAssignableTo<TBase>(Type type, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(type, parameterName);

        if (type.IsAbstract || !typeof(TBase).IsAssignableFrom(type))
        {
            throw new ArgumentException(
                $"Type '{type.FullName}' must be a non-abstract type derived from '{typeof(TBase).FullName}'.",
                parameterName);
        }

        return type;
    }
}
