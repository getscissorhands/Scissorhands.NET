namespace ScissorHands.Core.Manifests;

/// <summary>
/// This represents the manifest entity for website.
/// </summary>
public sealed class SiteManifest
{
    /// <summary>
    /// Defines the content directory name.
    /// </summary>
    public const string CONTENT_DIRECTORY = "contents";

    /// <summary>
    /// Defines the output directory name.
    /// </summary>
    public const string BUILD_OUTPUT_DIRECTORY = "dist";

    /// <summary>
    /// Defines the preview output directory name.
    /// </summary>
    public const string PREVIEW_OUTPUT_DIRECTORY = "preview";

    /// <summary>
    /// Gets or sets the generator of the site.
    /// </summary>
    public string Generator { get; set; } = "ScissorHands.NET";

    /// <summary>
    /// Gets the site title.
    /// </summary>
    public string Title { get; init; } = "ScissorHands.NET";

    /// <summary>
    /// Gets or sets the site description.
    /// </summary>
    public string Description { get; set; } = "A Blazor-based static site generator.";

    /// <summary>
    /// Gets the site author.
    /// </summary>
    public string Author { get; init; } = "The ScissorHands";

    /// <summary>
    /// Gets the site theme slug.
    /// </summary>
    public string Theme { get; init; } = string.Empty;

    /// <summary>
    /// Gets the content root directory.
    /// </summary>
    public string ContentRoot { get; init; } = CONTENT_DIRECTORY;

    /// <summary>
    /// Gets the build output directory.
    /// </summary>
    public string Output { get; init; } = BUILD_OUTPUT_DIRECTORY;

    /// <summary>
    /// Gets the preview output directory.
    /// </summary>
    public string PreviewOutput { get; init; } = PREVIEW_OUTPUT_DIRECTORY;

    /// <summary>
    /// Gets the base URL of the site.
    /// </summary>
    public string BaseUrl { get; init; } = "/";

    /// <summary>
    /// Gets a value indicating whether to include date in post URL.
    /// </summary>
    public bool IncludeDateInPostUrl { get; init; }
}
