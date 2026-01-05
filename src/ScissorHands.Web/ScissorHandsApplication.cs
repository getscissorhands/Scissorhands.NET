using System.Reflection;
using System.Runtime.ExceptionServices;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Options;
using ScissorHands.Web.Abstractions;
using ScissorHands.Web.Application;
using ScissorHands.Web.Generators;

namespace ScissorHands.Web;

/// <summary>
/// This provides the interface for ScissorHands application.
/// </summary>
public interface IScissorHandsApplication
{
    /// <summary>
    /// Runs the application.
    /// </summary>
    Task RunAsync();
}

/// <summary>
/// This represents the ScissorHands application entity.
/// </summary>
public sealed class ScissorHandsApplication : IScissorHandsApplication
{
    private const string APP_LOGGER_NAME = "App";
    private const int EXPECTED_GENERIC_PARAMETER_COUNT = 7;
    private const int EXPECTED_METHOD_PARAMETER_COUNT = 3;

    private readonly string[] _args;
    private readonly Type _mainLayout;
    private readonly Type _indexView;
    private readonly Type _postView;
    private readonly Type _pageView;
    private readonly Type _notFoundView;
    private readonly Type _tagListView;
    private readonly Type _tagView;
    private CommandMode _mode;
    private readonly WebApplication _app;
    private ILogger? _logger;
    private SiteManifest? _site;
    private IStaticSiteGenerator? _generator;
    private MethodInfo? _cachedBuildMethod;
    private readonly object _cacheLock = new object();

    internal ScissorHandsApplication(WebApplication app, IEnumerable<string> args, Type mainLayout, Type indexView, Type postView, Type pageView, Type notFoundView, Type tagListView, Type tagView)
    {
        _app = app ?? throw new ArgumentNullException(nameof(app));
        _args = args?.ToArray() ?? throw new ArgumentNullException(nameof(args));
        _mainLayout = mainLayout ?? throw new ArgumentNullException(nameof(mainLayout));
        _indexView = indexView ?? throw new ArgumentNullException(nameof(indexView));
        _postView = postView ?? throw new ArgumentNullException(nameof(postView));
        _pageView = pageView ?? throw new ArgumentNullException(nameof(pageView));
        _notFoundView = notFoundView ?? throw new ArgumentNullException(nameof(notFoundView));
        _tagListView = tagListView ?? throw new ArgumentNullException(nameof(tagListView));
        _tagView = tagView ?? throw new ArgumentNullException(nameof(tagView));
    }

    private void VerifyCommandArguments()
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
            Console.WriteLine("ERROR: No parameter or invalid parameter provided.");
            Console.ResetColor();
            DisplayHelp();

            Environment.Exit(1);
        }

        _mode = validation.Mode;
    }

    /// <inheritdoc />
    public async Task RunAsync()
    {
        VerifyCommandArguments();

        _logger = _app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(APP_LOGGER_NAME);
        _site = _app.Services.GetRequiredService<SiteManifest>();
        _generator = _app.Services.GetRequiredService<IStaticSiteGenerator>();

        _ = _mode switch
        {
            CommandMode.Preview => await RunPreviewServerAsync(),
            CommandMode.Build => await RunBuildAsync(),
            _ => LogInvalidMode()
        };

        if (_mode == CommandMode.Preview)
        {
            await _app.RunAsync();
        }
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

        await BuildSiteAsync(previewPath, preview: true, _app.Lifetime.ApplicationStopping);

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
                await BuildSiteAsync(previewPath, preview: true, CancellationToken.None);
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

        await BuildSiteAsync(outputPath, preview: false, CancellationToken.None);
        _logger!.LogInformation("Build complete. Output at {OutputPath}", outputPath);

        return this;
    }

    private Task BuildSiteAsync(string destination, bool preview, CancellationToken cancellationToken)
    {
        try
        {
            // Use cached method info if available, otherwise find and cache it
            if (_cachedBuildMethod == null)
            {
                lock (_cacheLock)
                {
                    if (_cachedBuildMethod == null)
                    {
                        _cachedBuildMethod = GetBuildAsyncMethod();
                    }
                }
            }

            var closedMethod = _cachedBuildMethod.MakeGenericMethod(_mainLayout, _indexView, _postView, _pageView, _notFoundView, _tagListView, _tagView);
            var parameters = new object[] { destination, preview, cancellationToken };
            var task = (Task?)closedMethod.Invoke(_generator, parameters);

            return task ?? Task.CompletedTask;
        }
        catch (InvalidOperationException ex)
        {
            _logger?.LogError(ex, "Failed to invoke BuildAsync method. The method signature may have changed or multiple overloads exist.");
            throw new InvalidOperationException("Failed to invoke the BuildAsync method on the static site generator. Please ensure the IStaticSiteGenerator interface has not changed.", ex);
        }
        catch (TargetInvocationException ex)
        {
            _logger?.LogError(ex.InnerException ?? ex, "BuildAsync method threw an exception during execution.");
            
            // Re-throw the inner exception while preserving the stack trace
            if (ex.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            }
            throw;
        }
        catch (ArgumentException ex)
        {
            _logger?.LogError(ex, "Failed to create generic method with the provided type arguments.");
            throw new InvalidOperationException("Failed to create the generic BuildAsync method. The provided view types may not satisfy the required constraints.", ex);
        }
    }

    private MethodInfo GetBuildAsyncMethod()
    {
        if (_generator == null)
        {
            throw new InvalidOperationException("Static site generator has not been initialized.");
        }

        var generatorType = _generator.GetType();
        var methods = generatorType.GetMethods()
            .Where(m => string.Equals(m.Name, nameof(IStaticSiteGenerator.BuildAsync), StringComparison.Ordinal) &&
                       m.IsGenericMethodDefinition)
            .ToList();

        if (methods.Count == 0)
        {
            throw new InvalidOperationException($"No BuildAsync method found on type {generatorType.Name}. Ensure the generator implements IStaticSiteGenerator.");
        }

        // Filter to the expected signature: 5 generic parameters, 3 method parameters
        var matchingMethods = methods.Where(m => m.GetGenericArguments().Length == EXPECTED_GENERIC_PARAMETER_COUNT &&
                                                  m.GetParameters().Length == EXPECTED_METHOD_PARAMETER_COUNT)
            .ToList();

        if (matchingMethods.Count == 0)
        {
            throw new InvalidOperationException($"No BuildAsync method with the expected signature ({EXPECTED_GENERIC_PARAMETER_COUNT} generic parameters, {EXPECTED_METHOD_PARAMETER_COUNT} method parameters) found on type {generatorType.Name}.");
        }

        if (matchingMethods.Count > 1)
        {
            throw new InvalidOperationException($"Multiple BuildAsync methods with the expected signature found on type {generatorType.Name}. Cannot determine which method to invoke.");
        }

        return matchingMethods[0];
    }

    private IScissorHandsApplication LogInvalidMode()
    {
        _logger!.LogError("Invalid command mode: {Mode}", _mode);

        return this;
    }
}