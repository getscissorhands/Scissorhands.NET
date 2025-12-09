using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;

namespace ScissorHands.Web.Infrastructure;

public sealed class ContentWatcher : IDisposable
{
    private readonly ILogger<ContentWatcher> _logger;
    private readonly FileSystemWatcher _contentWatcher;
    private readonly FileSystemWatcher _themeWatcher;
    private readonly Subject<string> _changes = new();
    private readonly IDisposable _subscription;

    public ContentWatcher(string contentPath, string themePath, TimeSpan debounce, Func<Task> onChange, ILogger<ContentWatcher> logger)
    {
        _logger = logger;

        _contentWatcher = CreateWatcher(contentPath);
        _themeWatcher = CreateWatcher(themePath);

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
