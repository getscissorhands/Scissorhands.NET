namespace ScissorHands.Core.Manifests;

/// <summary>
/// This represents the manifest entity for website.
/// </summary>
public sealed class SiteManifest
{
    private const string SITE_TITLE = "ScissorHands.NET";
    private const string SITE_LOCALE = "en-US";
    private const string SITE_URL = "http://localhost:5000";
    private const string BASE_URL = "/";

    /// <summary>
    /// Defines the contents directory.
    /// </summary>
    public const string CONTENTS_DIRECTORY = "contents";

    /// <summary>
    /// Defines the build output directory.
    /// </summary>
    public const string BUILD_OUTPUT_DIRECTORY = "dist";

    /// <summary>
    /// Defines the preview output directory.
    /// </summary>
    public const string PREVIEW_OUTPUT_DIRECTORY = "preview";

    /// <summary>
    /// Gets or sets the generator of the site.
    /// </summary>
    public string Generator { get; set; } = SITE_TITLE;

    /// <summary>
    /// Gets the site title.
    /// </summary>
    public string Title { get; init; } = SITE_TITLE;

    /// <summary>
    /// Gets or sets the site description.
    /// </summary>
    public string Description { get; set; } = "A Blazor-based static site generator.";

    /// <summary>
    /// Gets or sets the site description.
    /// </summary>
    public string? DescriptionInHtml { get; set; }

    /// <summary>
    /// Gets the site locale.
    /// </summary>
    public string Locale { get; init; } = SITE_LOCALE;

    /// <summary>
    /// Gets the site author.
    /// </summary>
    public string Author { get; init; } = "The ScissorHands";

    /// <summary>
    /// Gets the site theme slug.
    /// </summary>
    public string Theme { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the site URL.
    /// </summary>
    public string SiteUrl { get; set; } = SITE_URL;

    /// <summary>
    /// Gets the base URL of the site.
    /// </summary>
    public string BaseUrl { get; init; } = BASE_URL;

    /// <summary>
    /// Gets the site hero image.
    /// </summary>
    public string? HeroImage { get; init; }

    /// <summary>
    /// Gets a value indicating whether to include date in post URL or not.
    /// </summary>
    public bool IncludeDateInPostUrl { get; init; }

    /// <summary>
    /// Gets a value indicating whether to enable debug mode or not.
    /// </summary>
    public bool Debug { get; init; }
}
