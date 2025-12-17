using System.Text.RegularExpressions;

using Markdig;

using ScissorHands.Core.Services;

namespace ScissorHands.Web.Services;

/// <summary>
/// This represents the service entity for markdown.
/// </summary>
public sealed class MarkdownService : IMarkdownService
{
    private static readonly Regex trimRegex = new(@"^<p>(.*)</p>\s*$", RegexOptions.Singleline);

    private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
                                                      .UseAdvancedExtensions()
                                                      .UseSmartyPants()
                                                      .Build();

    /// <inheritdoc/>
    public Task<string> ToHtmlAsync(string markdown, bool? trim = false, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var html = Markdown.ToHtml(markdown, _pipeline);
        if (trim == true)
        {
            html = trimRegex.Replace(html, "$1");
        }

        return Task.FromResult(html);
    }
}
