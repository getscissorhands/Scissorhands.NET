using Markdig;

using ScissorHands.Core.Services;

namespace ScissorHands.Web.Services;

public sealed class MarkdownService : IMarkdownService
{
    private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
                                                      .UseAdvancedExtensions()
                                                      .UseSmartyPants()
                                                      .Build();

    public Task<string> ToHtmlAsync(string markdown, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var html = Markdown.ToHtml(markdown, _pipeline);

        return Task.FromResult(html);
    }
}
