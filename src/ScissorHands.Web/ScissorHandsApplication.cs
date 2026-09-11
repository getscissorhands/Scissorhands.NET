using System.Reflection;
using System.Runtime.ExceptionServices;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Options;
using ScissorHands.Theme;
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
    private ThemeComponentSet? _themeComponents;
    private CommandMode _mode;
    private readonly WebApplication _app;
    private ILogger? _logger;
    private SiteManifest? _site;
    private IStaticSiteGenerator? _generator;
    private MethodInfo? _cachedBuildMethod;
    private readonly object _cacheLock = new object();

    internal ScissorHandsApplication(WebApplication app, IEnumerable<string> args, ThemeComponentSet? themeComponents)
    {
        _app = app ?? throw new ArgumentNullException(nameof(app));
        _args = args?.ToArray() ?? throw new ArgumentNullException(nameof(args));
        _themeComponents = themeComponents;
    }

    private bool VerifyCommandArguments()
    {
        DisplayBanner();

        var validation = CommandArgumentValidator.Validate(_args);
        if (validation.IsHelp)
        {
            DisplayHelp();
            return false;
        }

        if (validation.IsError)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("ERROR: No parameter or invalid parameter provided.");
            Console.ResetColor();
            DisplayHelp();

            Environment.ExitCode = 1;
            return false;
        }

        _mode = validation.Mode;
        return true;
    }

    /// <inheritdoc />
    public async Task RunAsync()
    {
        if (!VerifyCommandArguments())
        {
            return;
        }

        _logger = _app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(APP_LOGGER_NAME);
        _site = _app.Services.GetRequiredService<SiteManifest>();
        _generator = _app.Services.GetRequiredService<IStaticSiteGenerator>();
        _themeComponents ??= _app.Services.GetRequiredService<IThemeComponentResolver>().Resolve(_site.Theme);

        await (_mode switch
        {
            CommandMode.Preview => RunPreviewServerAsync(),
            CommandMode.Build => RunBuildAsync(),
            _ => Task.FromResult(LogInvalidMode())
        });
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
        var banner = new[] {
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

        for (var i = 0; i < banner.Length; i++)
        {
            if (!Console.IsOutputRedirected)
            {
                Console.ForegroundColor = colors[i % colors.Length];
            }

            Console.WriteLine(banner[i]);
        }

        if (!Console.IsOutputRedirected)
        {
            Console.ResetColor();
        }

        Console.WriteLine();
    }

    private async Task<IScissorHandsApplication> RunPreviewServerAsync()
    {
        var previewPath = Path.GetFullPath(SiteManifest.PREVIEW_OUTPUT_DIRECTORY);
        if (Directory.Exists(previewPath))
        {
            Directory.Delete(previewPath, recursive: true);
        }

        Directory.CreateDirectory(previewPath);

        var fileProvider = new PhysicalFileProvider(previewPath);
        _app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = fileProvider });
        _app.UseStaticFiles(new StaticFileOptions { FileProvider = fileProvider });

        await _app.StartAsync();
        try
        {
            var addresses = _app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()?.Addresses;
            var siteUrl = addresses?.FirstOrDefault()?.TrimEnd('/')
                          ?? throw new InvalidOperationException("The preview server did not report a listening address.");
            _site!.SiteUrl = siteUrl;

            await BuildSiteAsync(previewPath, preview: true, _app.Lifetime.ApplicationStopping);

            var paths = _app.Services.GetRequiredService<IAppPaths>();
            var watcherFactory = _app.Services.GetRequiredService<IContentWatcherFactory>();
            using var watcher = watcherFactory.Create(
                paths.GetContentsRoot(),
                paths.GetThemesRoot(),
                TimeSpan.FromMilliseconds(500),
                async () =>
                {
                    _logger!.LogInformation("Change detected; rebuilding preview...");
                    await BuildSiteAsync(previewPath, preview: true, _app.Lifetime.ApplicationStopping);
                    _logger!.LogInformation("Preview rebuilt. Refresh your browser to see the changes.");
                });

            _logger!.LogInformation("Preview server running at {Address}", $"{siteUrl}/{_site.BaseUrl.TrimStart('/')}");
            await _app.WaitForShutdownAsync();
        }
        finally
        {
            await _app.StopAsync();
        }

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

            var themeComponents = _themeComponents
                                  ?? throw new InvalidOperationException("Theme components have not been resolved.");
            var closedMethod = _cachedBuildMethod.MakeGenericMethod(
                themeComponents.MainLayout,
                themeComponents.IndexView,
                themeComponents.PostView,
                themeComponents.PageView,
                themeComponents.NotFoundView,
                themeComponents.TagListView,
                themeComponents.TagView);
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
