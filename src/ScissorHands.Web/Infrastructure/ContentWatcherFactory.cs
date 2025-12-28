using Microsoft.Extensions.Logging;

using ScissorHands.Web.Abstractions;
using ScissorHands.Web.Watchers;

namespace ScissorHands.Web.Infrastructure;

/// <summary>
/// This represents the factory entity for <see cref="ContentWatcher"/>.
/// </summary>
/// <param name="logger"><see cref="ILogger{T}"/> instance.</param>
public sealed class ContentWatcherFactory(ILogger<ContentWatcher> logger) : IContentWatcherFactory
{
    private readonly ILogger<ContentWatcher> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc />
    public ContentWatcher Create(string contentPath, string themePath, TimeSpan debounce, Func<Task> onChange)
        => new(contentPath, themePath, debounce, onChange, _logger);
}
