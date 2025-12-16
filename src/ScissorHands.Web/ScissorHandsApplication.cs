using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Options;
using ScissorHands.Web.Extensions;
using ScissorHands.Web.Generators;
using ScissorHands.Web.Watchers;

namespace ScissorHands.Web;

public interface IScissorHandsApplication
{
    IScissorHandsApplication VerifyCommandArguments();
    Task<IScissorHandsApplication> BuildAsync();
    Task RunAsync();
}

public class ScissorHandsApplication<TMainLayout, TIndexView, TPostView, TPageView>(params IEnumerable<string> args) : IScissorHandsApplication
    where TMainLayout : ScissorHands.Theme.MainLayoutBase
    where TIndexView : ScissorHands.Theme.IndexViewBase
    where TPostView : ScissorHands.Theme.PostViewBase
    where TPageView : ScissorHands.Theme.PageViewBase
{
    private const string APP_LOGGER_NAME = "App";

    private readonly IEnumerable<string> _args = [.. args];
    private CommandMode _mode;
    private WebApplication? _app;
    private ILogger? _logger;
    private SiteManifest? _site;
    private IStaticSiteGenerator? _generator;

    public IScissorHandsApplication VerifyCommandArguments()
    {
        DisplayBanner();

        var command = CommandOptions.Parse(_args);
        if (command.Mode is CommandMode.Help)
        {
            DisplayHelp();
            Environment.Exit(0);
        }

        if (command.Mode is CommandMode.Unknown)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("ERROR: No parameter provided.");
            Console.ResetColor();
            DisplayHelp();

            Environment.Exit(1);
        }

        _mode = command.Mode;

        return this;
    }

    public async Task<IScissorHandsApplication> BuildAsync()
    {
        var builder = WebApplication.CreateBuilder([.. _args]);

        var config = builder.Configuration;
        builder.Services.AddConfigurations(config)
                        .AddServices(config)
                        .AddRazorComponents();

        _app = builder.Build();

        _logger = _app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(APP_LOGGER_NAME);
        _site = _app.Services.GetRequiredService<SiteManifest>();
        _generator = _app.Services.GetRequiredService<IStaticSiteGenerator>();

        var result = _mode switch
        {
            CommandMode.Preview => await RunPreviewServerAsync(),
            CommandMode.Build => await RunBuildAsync(),
            _ => LogInvalidMode()
        };

        return result;
    }

    public async Task RunAsync()
    {
        if (_mode is not CommandMode.Preview)
        {
            return;
        }

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
        var previewPath = Path.GetFullPath(_site!.PreviewOutput);
        if (Directory.Exists(previewPath))
        {
            Directory.Delete(previewPath, recursive: true);
        }

        await _generator!.BuildAsync<TMainLayout, TIndexView, TPostView, TPageView>(previewPath, preview: true, _app!.Lifetime.ApplicationStopping);

        var fileProvider = new PhysicalFileProvider(previewPath);
        _app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = fileProvider });
        _app.UseStaticFiles(new StaticFileOptions { FileProvider = fileProvider });

        _app.Lifetime.ApplicationStarted.Register(() =>
        {
            var addresses = _app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()?.Addresses;
            _logger!.LogInformation("Preview server running at {Address}", $"{string.Join(", ", addresses ?? []).TrimEnd('/')}{_site!.BaseUrl}");
        });

        var contentRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), _site.ContentRoot));
        var themeRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), ThemeManifest.THEME_DIRECTORY));

        var watcher = new ContentWatcher(
            contentRoot,
            themeRoot,
            TimeSpan.FromMilliseconds(500),
            async () =>
            {
                _logger!.LogInformation("Change detected; rebuilding preview...");
                await _generator!.BuildAsync<TMainLayout, TIndexView, TPostView, TPageView>(previewPath, preview: true, CancellationToken.None);
                _logger!.LogInformation("Preview rebuilt. Refresh your browser to see the changes.");
            },
            _app.Services.GetRequiredService<ILogger<ContentWatcher>>());

        _app.Lifetime.ApplicationStopping.Register(() => watcher.Dispose());

        return this;
    }

    private async Task<IScissorHandsApplication> RunBuildAsync()
    {
        var outputPath = Path.GetFullPath(_site!.Output);
        if (Directory.Exists(outputPath))
        {
            Directory.Delete(outputPath, recursive: true);
        }

        await _generator!.BuildAsync<TMainLayout, TIndexView, TPostView, TPageView>(outputPath, preview: false, CancellationToken.None);
        _logger!.LogInformation("Build complete. Output at {OutputPath}", outputPath);

        return this;
    }

    private IScissorHandsApplication LogInvalidMode()
    {
        _logger!.LogError("Invalid command mode: {Mode}", _mode);

        return this;
    }
}