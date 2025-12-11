namespace ScissorHands.Core.Services;

public interface IMarkdownService
{
    Task<string> ToHtmlAsync(string markdown, CancellationToken cancellationToken = default);
}
