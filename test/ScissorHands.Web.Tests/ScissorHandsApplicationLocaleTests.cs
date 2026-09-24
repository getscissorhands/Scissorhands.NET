using System.Net;

using AngleSharp.Html.Parser;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Theme;
using ScissorHands.Web.Abstractions;
using ScissorHands.Web.Extensions;
using ScissorHands.Web.Generators;

namespace ScissorHands.Web.Tests;

[Collection("NonParallel")]
public class ScissorHandsApplicationLocaleTests
{
    [Theory]
    [InlineData("/", false)]
    [InlineData("/", true)]
    [InlineData("/docs", false)]
    [InlineData("/docs", true)]
    [InlineData("/docs/", false)]
    [InlineData("/docs/", true)]
    [InlineData("/blog", false)]
    [InlineData("/blog", true)]
    [InlineData("/blog/", false)]
    [InlineData("/blog/", true)]
    [InlineData("/manual/docs", false)]
    [InlineData("/manual/docs", true)]
    [InlineData("/manual/docs/", false)]
    [InlineData("/manual/docs/", true)]
    public async Task Given_RealGeneratedSite_When_PreviewRuns_Then_It_Should_ComposeTheMountAndLocaleRoutes(
        string configuredBaseUrl, bool useLocale)
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var originalDirectory = Directory.GetCurrentDirectory();
        var temporaryDirectory = Directory.CreateTempSubdirectory("scissorhands-locale-preview-");
        try
        {
            Directory.SetCurrentDirectory(temporaryDirectory.FullName);
            await AddContent("posts", "hello.md",
                "title: English post\nslug: hello\npublished: 2026-09-16\ntags: [shared]");
            await AddContent("pages", "01-start.md",
                "title: English start\nslug: start\nshow_in_navigation: true");
            await AddContent("pages", "02-next.md",
                "title: English next\nslug: next\nshow_in_navigation: true");
            await AddContent("pages", "not-found.md", "title: Missing\nslug: 404.html");
            if (useLocale)
            {
                await AddContent("posts", "ko-kr/hello.md",
                    "title: Korean post\nslug: hello\npublished: 2026-09-16\ntags: [shared, korean-only]");
                await AddContent("pages", "ko-kr/01-start.md",
                    "title: Korean start\nslug: start\nshow_in_navigation: true");
            }
            var images = Directory.CreateDirectory(Path.Combine(temporaryDirectory.FullName, "contents", "images"));
            await File.WriteAllTextAsync(Path.Combine(images.FullName, "sample.svg"),
                "<svg xmlns=\"http://www.w3.org/2000/svg\"></svg>", cancellationToken);

            var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            Func<Task>? rebuild = null;
            var watcherFactory = Substitute.For<IContentWatcherFactory>();
            watcherFactory.When(factory => factory.Create(
                    Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<Func<Task>>()))
                .Do(call =>
                {
                    rebuild = call.ArgAt<Func<Task>>(3);
                    ready.TrySetResult();
                });
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { ContentRootPath = temporaryDirectory.FullName });
            builder.Configuration.Sources.Clear();
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Site:Title"] = "Locale integration",
                ["Site:BaseUrl"] = configuredBaseUrl,
                ["Site:Locale"] = useLocale ? "en-US" : null,
                ["Site:Theme"] = "default",
                ["Site:LocalizationFallbackMessages:ko-kr"] = "Korean translation unavailable.",
                ["Site:UseDateInPostUrl"] = "true",
            });
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Logging.ClearProviders();
            builder.Services.AddConfigurations(builder.Configuration)
                .AddServices(builder.Configuration, pluginAssemblies: [])
                .AddRazorComponents();
            builder.Services.AddSingleton(watcherFactory);
            var app = builder.Build();
            var components = new ThemeComponentSet(typeof(MainLayout), typeof(IndexView), typeof(PostView), typeof(PageView),
                typeof(NotFoundView), typeof(TagListView), typeof(TagView));
            var application = new ScissorHandsApplication(app, ["--preview"], components);
            Task? running = null;
            try
            {
                var generator = app.Services.GetRequiredService<IStaticSiteGenerator>();
                var dist = Path.Combine(temporaryDirectory.FullName, "dist");
                await generator.BuildAsync<MainLayout, IndexView, PostView, PageView, NotFoundView, TagListView, TagView>(
                    dist, false, cancellationToken);
                running = application.RunAsync();
                var started = await Task.WhenAny(ready.Task, running).WaitAsync(TimeSpan.FromSeconds(15), cancellationToken);
                await started;
                ready.Task.IsCompletedSuccessfully.ShouldBeTrue();
                var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
                var site = app.Services.GetRequiredService<SiteManifest>();
                var baseUrl = configuredBaseUrl.EndsWith('/') ? configuredBaseUrl : configuredBaseUrl + "/";
                site.BaseUrl.ShouldBe(baseUrl);
                site.Locale.ShouldBe(useLocale ? "en-US" : null);
                builder.Configuration["Site:BaseUrl"].ShouldBe(configuredBaseUrl);
                using var handler = new HttpClientHandler { AllowAutoRedirect = false };
                using var client = new HttpClient(handler) { BaseAddress = new Uri(address), Timeout = TimeSpan.FromSeconds(10) };
                var localePrefix = string.Empty;
                var parser = new HtmlParser();

                foreach (var method in new[] { HttpMethod.Get, HttpMethod.Head })
                {
                    using var rootRequest = new HttpRequestMessage(method, "/?from=preview");
                    using var rootResponse = await client.SendAsync(rootRequest, cancellationToken);
                    rootResponse.StatusCode.ShouldBe(baseUrl == "/" ? HttpStatusCode.OK : HttpStatusCode.Found);
                    if (baseUrl != "/")
                    {
                        rootResponse.Headers.Location.ShouldBe(new Uri(baseUrl + "?from=preview", UriKind.Relative));
                        (await rootResponse.Content.ReadAsStringAsync(cancellationToken)).ShouldBeEmpty();
                    }
                }

                var entry = await client.GetStringAsync(baseUrl, cancellationToken);
                using var entryHtml = parser.ParseDocument(entry);
                entryHtml.QuerySelector("meta[http-equiv='refresh']").ShouldBeNull();
                using var home = parser.ParseDocument(await client.GetStringAsync($"{baseUrl}{localePrefix}", cancellationToken));
                home.Title.ShouldBe("Locale integration");
                home.QuerySelector("base")!.GetAttribute("href").ShouldBe(baseUrl);
                home.QuerySelectorAll(".post-link").Select(link => link.TextContent).ShouldBe(["English post"]);
                home.QuerySelectorAll(".site-header nav a").Select(link => link.TextContent)
                    .ShouldBe(["Home", "English start", "English next", "Tags"]);
                using var buildHome = parser.ParseDocument(await File.ReadAllTextAsync(
                    Path.Combine(dist, localePrefix.Replace('/', Path.DirectorySeparatorChar), "index.html"), cancellationToken));
                buildHome.QuerySelector("base")!.GetAttribute("href").ShouldBe(baseUrl);

                using var page = parser.ParseDocument(await client.GetStringAsync($"{baseUrl}{localePrefix}start/", cancellationToken));
                var next = page.QuerySelector(".page-navigation-next")!.GetAttribute("href")!;
                next.ShouldBe(localePrefix + "next");
                using var nextResponse = await client.GetAsync(new Uri(new Uri(address + baseUrl), next), cancellationToken);
                nextResponse.StatusCode.ShouldBe(HttpStatusCode.MovedPermanently);
                nextResponse.Headers.Location.ShouldBe(new Uri($"{address}{baseUrl}{localePrefix}next/"));
                (await client.GetStringAsync(nextResponse.Headers.Location, cancellationToken)).ShouldContain("English next");
                using var datedPost = parser.ParseDocument(await client.GetStringAsync(
                    $"{baseUrl}{localePrefix}2026/09/16/hello/", cancellationToken));
                datedPost.QuerySelector(".back-link")!.GetAttribute("href").ShouldBe(".");
                using var tag = parser.ParseDocument(await client.GetStringAsync($"{baseUrl}{localePrefix}tags/shared/", cancellationToken));
                tag.QuerySelectorAll(".post-link").Select(link => link.TextContent).ShouldBe(["English post"]);
                tag.QuerySelector(".back-link")!.GetAttribute("href").ShouldBe(localePrefix + "tags");

                if (useLocale)
                {
                    using var korean = parser.ParseDocument(await client.GetStringAsync($"{baseUrl}ko-kr/", cancellationToken));
                    korean.QuerySelectorAll(".post-link").Select(link => link.TextContent).ShouldBe(["Korean post"]);
                    korean.QuerySelectorAll(".site-header nav a").Select(link => link.TextContent)
                        .ShouldBe(["Home", "Korean start", "English next", "Tags"]);
                    foreach (var route in new[] { "tags", "tags/shared" })
                    {
                        using var primaryTags = parser.ParseDocument(await client.GetStringAsync($"{baseUrl}{route}/", cancellationToken));
                        primaryTags.QuerySelector("meta[http-equiv='refresh']").ShouldBeNull();
                    }
                    using var absentLegacy = await client.GetAsync($"{baseUrl}tags/korean-only/", cancellationToken);
                    absentLegacy.StatusCode.ShouldBe(HttpStatusCode.NotFound);
                    using var koreanTag = await client.GetAsync($"{baseUrl}ko-kr/tags/korean-only/", cancellationToken);
                    koreanTag.StatusCode.ShouldBe(HttpStatusCode.OK);
                    using var localizedPage = parser.ParseDocument(await client.GetStringAsync($"{baseUrl}ko-kr/start/", cancellationToken));
                    localizedPage.QuerySelectorAll("article a").Select(a => a.GetAttribute("href"))
                        .ShouldBe(["ko-kr/next/?q=a%2Fb#content", "next/?q=a%2Fb#content"]);
                    using var buildPage = parser.ParseDocument(await File.ReadAllTextAsync(
                        Path.Combine(dist, "ko-kr", "start", "index.html"), cancellationToken));
                    buildPage.QuerySelectorAll("article a").Select(a => a.GetAttribute("href"))
                        .ShouldBe(localizedPage.QuerySelectorAll("article a").Select(a => a.GetAttribute("href")));
                    foreach (var route in new[] { "ko-kr/start/", "ko-kr/next/", "ko-kr/", "ko-kr/tags/", "ko-kr/tags/korean-only/", "404.html" })
                    {
                        using var switched = parser.ParseDocument(await client.GetStringAsync(baseUrl + route, cancellationToken));
                        foreach (var link in switched.QuerySelectorAll(".language-switcher a"))
                        {
                            using var destination = await client.GetAsync(new Uri(new Uri(address + baseUrl), link.GetAttribute("href")!), cancellationToken);
                            destination.StatusCode.ShouldBe(HttpStatusCode.OK);
                        }
                    }
                }
                else
                {
                    home.QuerySelector(".language-switcher").ShouldBeNull();
                    page.QuerySelector("article a")!.GetAttribute("href").ShouldBe("next/?q=a%2Fb#content");
                }
                foreach (var route in new[] { "404.html", "images/sample.svg", "themes/default/assets/theme.css", "themes/default/assets/theme.js" })
                {
                    using var response = await client.GetAsync(baseUrl + route, cancellationToken);
                    response.StatusCode.ShouldBe(HttpStatusCode.OK, route);
                    if (baseUrl != "/")
                    {
                        using var outside = await client.GetAsync("/" + route, cancellationToken);
                        outside.StatusCode.ShouldBe(HttpStatusCode.NotFound, route);
                    }
                }
                using var cssHead = new HttpRequestMessage(HttpMethod.Head, baseUrl + "themes/default/assets/theme.css");
                using var cssResponse = await client.SendAsync(cssHead, cancellationToken);
                cssResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
                (await cssResponse.Content.ReadAsStringAsync(cancellationToken)).ShouldBeEmpty();
                if (baseUrl != "/")
                {
                    foreach (var route in new[] { $"/{localePrefix}start/", $"/{localePrefix}tags/shared/", $"{baseUrl}{baseUrl.Trim('/')}/{localePrefix}start/" })
                    {
                        using var response = await client.GetAsync(route, cancellationToken);
                        response.StatusCode.ShouldBe(HttpStatusCode.NotFound, route);
                    }
                }
                var preview = Path.Combine(temporaryDirectory.FullName, "preview");
                Files(preview).ShouldBe(Files(dist));
                if (baseUrl != "/")
                {
                    Directory.Exists(Path.Combine(preview, baseUrl.Trim('/'))).ShouldBeFalse();
                }

                await AddContent("pages", "03-third.md",
                    "title: English third\nslug: third\nshow_in_navigation: true");
                await rebuild!();
                using var updated = parser.ParseDocument(await client.GetStringAsync($"{baseUrl}{localePrefix}next/", cancellationToken));
                updated.QuerySelector(".page-navigation-next")!.GetAttribute("href").ShouldBe(localePrefix + "third");
                using var third = await client.GetAsync($"{baseUrl}{localePrefix}third/", cancellationToken);
                third.StatusCode.ShouldBe(HttpStatusCode.OK);
                if (useLocale)
                {
                    var korean = await client.GetStringAsync($"{baseUrl}ko-kr/start/", cancellationToken);
                    korean.ShouldContain("English third");
                    using var fallback = parser.ParseDocument(await client.GetStringAsync($"{baseUrl}ko-kr/third/", cancellationToken));
                    fallback.QuerySelector("[data-localization-fallback]")!.TextContent.ShouldBe("Korean translation unavailable.");
                }
                File.Delete(Path.Combine(temporaryDirectory.FullName, "contents", "pages", "03-third.md"));
                await rebuild!();
                using var removed = await client.GetAsync($"{baseUrl}third/", cancellationToken);
                removed.StatusCode.ShouldBe(HttpStatusCode.NotFound);
                using var removedTranslation = await client.GetAsync($"{baseUrl}ko-kr/third/", cancellationToken);
                removedTranslation.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            }
            finally
            {
                app.Lifetime.StopApplication();
                try
                {
                    if (running is not null)
                    {
                        await running.WaitAsync(TimeSpan.FromSeconds(15), CancellationToken.None);
                    }
                }
                finally
                {
                    await app.DisposeAsync();
                }
            }

            async Task AddContent(string kind, string path, string metadata)
            {
                var filename = Path.Combine(temporaryDirectory.FullName, "contents", kind, path.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(filename)!);
                await File.WriteAllTextAsync(filename,
                    $"---\n{metadata}\n---\n# Content\n\n[Next](next/?q=a%2Fb#content)\n[Primary](next/?q=a%2Fb#content){{data-localize=\"false\"}}",
                    cancellationToken);
            }

            static IEnumerable<string> Files(string root) => Directory.GetFiles(root, "*", SearchOption.AllDirectories)
                .Select(file => Path.GetRelativePath(root, file)).Order(StringComparer.Ordinal);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
            temporaryDirectory.Delete(recursive: true);
        }
    }
}
