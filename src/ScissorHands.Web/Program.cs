using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScissorHands.Web.Infrastructure;
using ScissorHands.Web.Models;
using ScissorHands.Web.Rendering;
using ScissorHands.Web.Services;

var cli = CliOptions.Parse(args);
if (cli.Mode is CliMode.Unknown)
{
    Console.Error.WriteLine("Specify a mode: --preview to run preview server, or --build for static build.");
    return;
}
var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection("Site"));
builder.Services.Configure<List<PluginDefinition>>(builder.Configuration.GetSection("Plugins"));

builder.Services.AddRazorComponents();

builder.Services.AddSingleton<ContentLoader>();
builder.Services.AddSingleton<MarkdownService>();
builder.Services.AddSingleton<PluginLoader>();
builder.Services.AddSingleton<PluginRunner>();
builder.Services.AddSingleton<ThemeService>();
builder.Services.AddSingleton<ComponentRenderer>();
builder.Services.AddSingleton<StaticSiteGenerator>();

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
var siteOptions = app.Services.GetRequiredService<IOptions<SiteOptions>>().Value;
var generator = app.Services.GetRequiredService<StaticSiteGenerator>();
var contentRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), siteOptions.ContentRoot));
var themeRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "themes"));

if (cli.Mode is CliMode.Preview)
{
    var previewPath = Path.GetFullPath(siteOptions.PreviewOutput);
    if (Directory.Exists(previewPath))
    {
        Directory.Delete(previewPath, recursive: true);
    }
    await generator.BuildAsync(previewPath, preview: true, app.Lifetime.ApplicationStopping);

    // serve generated preview files
    var fileProvider = new PhysicalFileProvider(previewPath);
    app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = fileProvider });
    app.UseStaticFiles(new StaticFileOptions { FileProvider = fileProvider });

    app.Lifetime.ApplicationStarted.Register(() =>
    {
        var addresses = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()?.Addresses;
        logger.LogInformation("Preview server running at {Address}", $"{string.Join(", ", addresses ?? Array.Empty<string>()).TrimEnd('/')}{siteOptions.BaseUrl}");
    });

    var watcher = new ContentWatcher(
        contentRoot,
        themeRoot,
        TimeSpan.FromMilliseconds(500),
        async () =>
        {
            logger.LogInformation("Change detected; rebuilding preview...");
            await generator.BuildAsync(previewPath, preview: true, CancellationToken.None);
        },
        app.Services.GetRequiredService<ILogger<ContentWatcher>>());

    app.Lifetime.ApplicationStopping.Register(() => watcher.Dispose());
    await app.RunAsync();
}
else
{
    var outputPath = Path.GetFullPath(siteOptions.Output);
    if (Directory.Exists(outputPath))
    {
        Directory.Delete(outputPath, recursive: true);
    }

    await generator.BuildAsync(outputPath, preview: false, CancellationToken.None);
    logger.LogInformation("Build complete. Output at {OutputPath}", outputPath);
}

internal enum CliMode
{
    Unknown,
    Preview,
    Build
}

internal sealed record CliOptions(CliMode Mode)
{
    public static CliOptions Parse(string[] args)
    {
        if (args.Any(a => string.Equals(a, "--preview", StringComparison.OrdinalIgnoreCase) || string.Equals(a, "preview", StringComparison.OrdinalIgnoreCase)))
        {
            return new CliOptions(CliMode.Preview);
        }

        if (args.Any(a => string.Equals(a, "--build", StringComparison.OrdinalIgnoreCase) || string.Equals(a, "build", StringComparison.OrdinalIgnoreCase)))
        {
            return new CliOptions(CliMode.Build);
        }

        return new CliOptions(CliMode.Unknown);
    }
}
