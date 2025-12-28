using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Options;
using ScissorHands.Web.Application;
using ScissorHands.Web.Abstractions;
using ScissorHands.Web.Extensions;
using ScissorHands.Web.Generators;

namespace ScissorHands.Web;

/// <summary>
/// This provides the interface for ScissorHands application.
/// </summary>
public interface IScissorHandsApplication
{
    /// <summary>
    /// Verifies the command arguments.
    /// </summary>
    /// <returns>Returns the <see cref="IScissorHandsApplication"/> instance.</returns>
    IScissorHandsApplication VerifyCommandArguments();

    /// <summary>
    /// Builds the application.
    /// </summary>
    /// <returns>Returns the <see cref="IScissorHandsApplication"/> instance.</returns>
    Task<IScissorHandsApplication> BuildAsync();

    /// <summary>
    /// Runs the application.
    /// </summary>
    Task RunAsync();
}

/// <summary>
/// This represents the ScissorHands application entity.
/// </summary>
/// <typeparam name="TMainLayout">Type of main layout component.</typeparam>
/// <typeparam name="TIndexView">Type of index view component.</typeparam>
/// <typeparam name="TPostView">Type of post view component.</typeparam>
/// <typeparam name="TPageView">Type of page view component.</typeparam>
/// <typeparam name="TNotFoundView">Type of not found (404) view component.</typeparam>
public class ScissorHandsApplication<TMainLayout, TIndexView, TPostView, TPageView, TNotFoundView>(params IEnumerable<string> args) : IScissorHandsApplication
    where TMainLayout : ScissorHands.Theme.MainLayoutBase
    where TIndexView : ScissorHands.Theme.IndexViewBase
    where TPostView : ScissorHands.Theme.PostViewBase
    where TPageView : ScissorHands.Theme.PageViewBase
    where TNotFoundView : ScissorHands.Theme.NotFoundViewBase
{
    private const string APP_LOGGER_NAME = "App";

    private readonly IEnumerable<string> _args = [.. args];
    private CommandMode _mode;
    private WebApplication? _app;
    private ILogger? _logger;
    private SiteManifest? _site;
    private IStaticSiteGenerator? _generator;

    /// <inheritdoc />
    public IScissorHandsApplication VerifyCommandArguments()
    {
        DisplayBanner();

        var validation = CommandArgumentValidator.Validate(_args);
        if (validation.IsHelp)
        {
            DisplayHelp();
            Environment.Exit(0);
        }

        if (validation.IsError)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("ERROR: No parameter provided.");
            Console.ResetColor();
            DisplayHelp();

            Environment.Exit(1);
        }

        _mode = validation.Mode;

        return this;
    }

    /// <inheritdoc />
    public async Task<IScissorHandsApplication> BuildAsync()
    {
        var builder = WebApplication.CreateBuilder([.. _args]);

        var config = builder.Configuration;
        builder.Services.AddConfigurations(config)
                        .AddServices(config)
                        .AddRazorComponents();

        _app = builder.Build();

        return this;
    }

    /// <inheritdoc />
    public async Task RunAsync()
    {
        if (_mode is not CommandMode.Preview)
        {
            return;
        }

        _logger = _app!.Services.GetRequiredService<ILoggerFactory>().CreateLogger(APP_LOGGER_NAME);
        _site = _app!.Services.GetRequiredService<SiteManifest>();
        _generator = _app!.Services.GetRequiredService<IStaticSiteGenerator>();

        var result = _mode switch
        {
            CommandMode.Preview => await RunPreviewServerAsync(),
            CommandMode.Build => await RunBuildAsync(),
            _ => LogInvalidMode()
        };

        await _app!.RunAsync();
    }

    private static void DisplayHelp()
    {
        Console.WriteLine();
        Console.WriteLine("Usage: dotnet run -- [--preview | --build | --help]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --preview    Run the preview server.");
        Console.WriteLine("  --build      Build the static site.");
        Console.WriteLine("  --help       Display this help message.");
        Console.WriteLine();
    }

    private static void DisplayBanner()
    {
        var banner = new [] {
            "███████╗ ██████╗██╗███████╗███████╗ ██████╗ ██████╗",
            "██╔════╝██╔════╝██║██╔════╝██╔════╝██╔═══██╗██╔══██╗",
            "███████╗██║     ██║███████╗███████╗██║   ██║██████╔╝",
            "╚════██║██║     ██║╚════██║╚════██║██║   ██║██╔══██╗",
            "███████║╚██████╗██║███████║███████║╚██████╔╝██║  ██║",
            "╚══════╝ ╚═════╝╚═╝╚══════╝╚══════╝ ╚═════╝ ╚═╝  ╚═╝",
            "██╗  ██╗ █████╗ ███╗   ██╗██████╗ ███████╗   ███╗   ██╗███████╗████████╗",
            "██║  ██║██╔══██╗████╗  ██║██╔══██╗██╔════╝   ████╗  ██║██╔════╝╚══██╔══╝",
            "███████║███████║██╔██╗ ██║██║  ██║███████╗   ██╔██╗ ██║█████╗     ██║",
            "██╔══██║██╔══██║██║╚██╗██║██║  ██║╚════██║   ██║╚██╗██║██╔══╝     ██║",
            "██║  ██║██║  ██║██║ ╚████║██████╔╝███████║██╗██║ ╚████║███████╗   ██║",
            "╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═══╝╚═════╝ ╚══════╝╚═╝╚═╝  ╚═══╝╚══════╝   ╚═╝"
        };

        var colors = new[]
        {
            ConsoleColor.Cyan,
            ConsoleColor.Blue,
            ConsoleColor.Magenta,
            ConsoleColor.Yellow,
            ConsoleColor.Green,
            ConsoleColor.Red
        };

        Console.WriteLine(new string('\n', Console.WindowHeight));

        for (var i = 0; i < banner.Length; i++)
        {
            Console.ForegroundColor = colors[i % colors.Length];
            Console.WriteLine(banner[i]);
        }

        Console.ResetColor();
        Console.WriteLine();
    }

    private async Task<IScissorHandsApplication> RunPreviewServerAsync()
    {
        var previewPath = Path.GetFullPath(SiteManifest.PREVIEW_OUTPUT_DIRECTORY);
        if (Directory.Exists(previewPath))
        {
            Directory.Delete(previewPath, recursive: true);
        }

        await _generator!.BuildAsync<TMainLayout, TIndexView, TPostView, TPageView, TNotFoundView>(previewPath, preview: true, _app!.Lifetime.ApplicationStopping);

        var fileProvider = new PhysicalFileProvider(previewPath);
        _app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = fileProvider });
        _app.UseStaticFiles(new StaticFileOptions { FileProvider = fileProvider });

        _app.Lifetime.ApplicationStarted.Register(() =>
        {
            var addresses = _app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()?.Addresses;
            var siteUrl = string.Join(", ", addresses ?? []).TrimEnd('/');
            _site!.SiteUrl = siteUrl;
            _logger!.LogInformation("Preview server running at {Address}", $"{siteUrl}/{_site!.BaseUrl.TrimStart('/')}");
        });

        var paths = _app.Services.GetRequiredService<IAppPaths>();
        var contentRoot = paths.GetContentsRoot();
        var themeRoot = paths.GetThemesRoot();

        var watcherFactory = _app.Services.GetRequiredService<IContentWatcherFactory>();
        var watcher = watcherFactory.Create(
            contentRoot,
            themeRoot,
            TimeSpan.FromMilliseconds(500),
            async () =>
            {
                _logger!.LogInformation("Change detected; rebuilding preview...");
                await _generator!.BuildAsync<TMainLayout, TIndexView, TPostView, TPageView, TNotFoundView>(previewPath, preview: true, CancellationToken.None);
                _logger!.LogInformation("Preview rebuilt. Refresh your browser to see the changes.");
            });

        _app.Lifetime.ApplicationStopping.Register(watcher.Dispose);

        return this;
    }

    private async Task<IScissorHandsApplication> RunBuildAsync()
    {
        var outputPath = Path.GetFullPath(SiteManifest.BUILD_OUTPUT_DIRECTORY);
        if (Directory.Exists(outputPath))
        {
            Directory.Delete(outputPath, recursive: true);
        }

        await _generator!.BuildAsync<TMainLayout, TIndexView, TPostView, TPageView, TNotFoundView>(outputPath, preview: false, CancellationToken.None);
        _logger!.LogInformation("Build complete. Output at {OutputPath}", outputPath);

        return this;
    }

    private IScissorHandsApplication LogInvalidMode()
    {
        _logger!.LogError("Invalid command mode: {Mode}", _mode);

        return this;
    }
}