using Markdig;
using Markdig.Syntax;

using ScissorHands.Core.Services;

namespace ScissorHands.Web.Services;

/// <summary>
/// This represents the service entity for markdown.
/// </summary>
public sealed class MarkdownService : IMarkdownService
{
    private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
                                                      .UseAdvancedExtensions()
                                                      .UseSmartyPants()
                                                      .Build();

    /// <inheritdoc/>
    public Task<string> ToHtmlAsync(string markdown, bool? trim = false, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var document = Markdown.Parse(markdown, _pipeline);
        var html = document.ToHtml(_pipeline);
        if (trim == true && document.Count == 1 && document[0] is ParagraphBlock)
        {
            var trimmedHtml = html.TrimEnd();
            if (trimmedHtml.StartsWith("<p>", StringComparison.Ordinal)
                && trimmedHtml.EndsWith("</p>", StringComparison.Ordinal))
            {
                html = trimmedHtml[3..^4];
            }
        }

        return Task.FromResult(html);
    }
}
