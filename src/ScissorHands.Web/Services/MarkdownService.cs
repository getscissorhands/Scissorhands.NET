using Markdig;

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
    public Task<string> ToHtmlAsync(string markdown, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var html = Markdown.ToHtml(markdown, _pipeline);

        return Task.FromResult(html);
    }
}
