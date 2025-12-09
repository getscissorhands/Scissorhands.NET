using Markdig;

namespace ScissorHands.Web.Services;

public sealed class MarkdownService
{
    private readonly MarkdownPipeline _pipeline;

    public MarkdownService()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseSmartyPants()
            .Build();
    }

    public Task<string> ToHtmlAsync(string markdown, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var html = Markdown.ToHtml(markdown, _pipeline);
        return Task.FromResult(html);
    }
}
