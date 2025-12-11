using System.Reactive.Linq;
using System.Reactive.Subjects;

using Microsoft.Extensions.Logging;

namespace ScissorHands.Web.Watchers;

public sealed class ContentWatcher : IDisposable
{
    private readonly FileSystemWatcher _contentWatcher;
    private readonly FileSystemWatcher _themeWatcher;
    private readonly ILogger<ContentWatcher> _logger;
    private readonly Subject<string> _changes = new();
    private readonly IDisposable _subscription;

    public ContentWatcher(string contentPath, string themePath, TimeSpan debounce, Func<Task> onChange, ILogger<ContentWatcher> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _contentWatcher = CreateWatcher(contentPath ?? throw new ArgumentNullException(nameof(contentPath)));
        _themeWatcher = CreateWatcher(themePath ?? throw new ArgumentNullException(nameof(themePath)));

        _subscription = _changes
            .Throttle(debounce)
            .Subscribe(async _ =>
            {
                try
                {
                    await onChange();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Watcher handler failed");
                }
            });

        _contentWatcher.Changed += (_, __) => _changes.OnNext("content");
        _contentWatcher.Created += (_, __) => _changes.OnNext("content");
        _contentWatcher.Deleted += (_, __) => _changes.OnNext("content");
        _contentWatcher.Renamed += (_, __) => _changes.OnNext("content");

        _themeWatcher.Changed += (_, __) => _changes.OnNext("theme");
        _themeWatcher.Created += (_, __) => _changes.OnNext("theme");
        _themeWatcher.Deleted += (_, __) => _changes.OnNext("theme");
        _themeWatcher.Renamed += (_, __) => _changes.OnNext("theme");

        _contentWatcher.EnableRaisingEvents = true;
        _themeWatcher.EnableRaisingEvents = true;
    }

    private static FileSystemWatcher CreateWatcher(string path)
    {
        var watcher = new FileSystemWatcher(path)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite
        };

        return watcher;
    }

    public void Dispose()
    {
        _subscription.Dispose();
        _contentWatcher.Dispose();
        _themeWatcher.Dispose();
        _changes.Dispose();
    }
}
