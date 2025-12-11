using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.FileProviders;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Options;
using ScissorHands.Core.Services;
using ScissorHands.Theme.MinimalBlog;
using ScissorHands.Web;
using ScissorHands.Web.Generators;
using ScissorHands.Web.Loaders;
using ScissorHands.Web.Renderers;
using ScissorHands.Web.Runners;
using ScissorHands.Web.Services;
using ScissorHands.Web.Watchers;

var app = await new ScissorHandsApplication<MainLayout, IndexView, PostView, PageView>(args)
                    .VerifyCommandArguments()
                    .InitializeAsync();
await app.RunAsync();



// var command = CommandOptions.Parse(args);
// if (command.Mode is CommandMode.Unknown)
// {
//     Console.Error.WriteLine("Specify a mode: --preview to run preview server, or --build for static build.");
//     return;
// }

// var banner = new [] {
//     "███████╗ ██████╗██╗███████╗███████╗ ██████╗ ██████╗",
//     "██╔════╝██╔════╝██║██╔════╝██╔════╝██╔═══██╗██╔══██╗",
//     "███████╗██║     ██║███████╗███████╗██║   ██║██████╔╝",
//     "╚════██║██║     ██║╚════██║╚════██║██║   ██║██╔══██╗",
//     "███████║╚██████╗██║███████║███████║╚██████╔╝██║  ██║",
//     "╚══════╝ ╚═════╝╚═╝╚══════╝╚══════╝ ╚═════╝ ╚═╝  ╚═╝",
//     "██╗  ██╗ █████╗ ███╗   ██╗██████╗ ███████╗   ███╗   ██╗███████╗████████╗",
//     "██║  ██║██╔══██╗████╗  ██║██╔══██╗██╔════╝   ████╗  ██║██╔════╝╚══██╔══╝",
//     "███████║███████║██╔██╗ ██║██║  ██║███████╗   ██╔██╗ ██║█████╗     ██║",
//     "██╔══██║██╔══██║██║╚██╗██║██║  ██║╚════██║   ██║╚██╗██║██╔══╝     ██║",
//     "██║  ██║██║  ██║██║ ╚████║██████╔╝███████║██╗██║ ╚████║███████╗   ██║",
//     "╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═══╝╚═════╝ ╚══════╝╚═╝╚═╝  ╚═══╝╚══════╝   ╚═╝"
// };

// var colors = new[]
// {
//     ConsoleColor.Cyan,
//     ConsoleColor.Blue,
//     ConsoleColor.Magenta,
//     ConsoleColor.Yellow,
//     ConsoleColor.Green,
//     ConsoleColor.Red
// };

// for (var i = 0; i < banner.Length; i++)
// {
//     Console.ForegroundColor = colors[i % colors.Length];
//     Console.WriteLine(banner[i]);
// }

// Console.ResetColor();

// var builder = WebApplication.CreateBuilder(args);

// var config = builder.Configuration;
// var siteManifest = config.GetSection("Site").Get<SiteManifest>();
// var pluginManifests = config.GetSection("Plugins").Get<List<PluginManifest>>();
// builder.Services.AddSingleton(siteManifest!);
// builder.Services.AddSingleton(pluginManifests ?? []);

// builder.Services.AddRazorComponents();

// builder.Services.AddSingleton<IContentLoader, ContentLoader>();
// builder.Services.AddSingleton<IMarkdownService, MarkdownService>();
// builder.Services.AddSingleton<IPluginLoader, PluginLoader>();
// builder.Services.AddSingleton<IPluginRunner, PluginRunner>();
// builder.Services.AddSingleton<IThemeService, ThemeService>();
// builder.Services.AddSingleton<IComponentRenderer, ComponentRenderer>();
// builder.Services.AddSingleton<IStaticSiteGenerator, StaticSiteGenerator>();

// var app = builder.Build();
// var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
// var siteOptions = app.Services.GetRequiredService<SiteManifest>();
// var generator = app.Services.GetRequiredService<IStaticSiteGenerator>();
// var contentRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), siteOptions.ContentRoot));
// var themeRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "themes"));

// if (command.Mode is CommandMode.Preview)
// {
//     var previewPath = Path.GetFullPath(siteOptions.PreviewOutput);
//     if (Directory.Exists(previewPath))
//     {
//         Directory.Delete(previewPath, recursive: true);
//     }
//     await generator.BuildAsync<MainLayout, IndexView, PostView, PageView>(previewPath, preview: true, app.Lifetime.ApplicationStopping);

//     // serve generated preview files
//     var fileProvider = new PhysicalFileProvider(previewPath);
//     app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = fileProvider });
//     app.UseStaticFiles(new StaticFileOptions { FileProvider = fileProvider });

//     app.Lifetime.ApplicationStarted.Register(() =>
//     {
//         var addresses = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()?.Addresses;
//         logger.LogInformation("Preview server running at {Address}", $"{string.Join(", ", addresses ?? Array.Empty<string>()).TrimEnd('/')}{siteOptions.BaseUrl}");
//     });

//     var watcher = new ContentWatcher(
//         contentRoot,
//         themeRoot,
//         TimeSpan.FromMilliseconds(500),
//         async () =>
//         {
//             logger.LogInformation("Change detected; rebuilding preview...");
//             await generator.BuildAsync<MainLayout, IndexView, PostView, PageView>(previewPath, preview: true, CancellationToken.None);
//             logger.LogInformation("Preview rebuilt. Refresh your browser to see the changes.");
//         },
//         app.Services.GetRequiredService<ILogger<ContentWatcher>>());

//     app.Lifetime.ApplicationStopping.Register(() => watcher.Dispose());
//     await app.RunAsync();
// }
// else
// {
//     var outputPath = Path.GetFullPath(siteOptions.Output);
//     if (Directory.Exists(outputPath))
//     {
//         Directory.Delete(outputPath, recursive: true);
//     }

//     await generator.BuildAsync<MainLayout, IndexView, PostView, PageView>(outputPath, preview: false, CancellationToken.None);
//     logger.LogInformation("Build complete. Output at {OutputPath}", outputPath);
// }
