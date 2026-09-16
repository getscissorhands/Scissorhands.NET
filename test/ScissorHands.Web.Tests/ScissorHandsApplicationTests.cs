using System.Net;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Theme;
using ScissorHands.Web.Abstractions;
using ScissorHands.Web.Generators;

namespace ScissorHands.Web.Tests;

[Collection("NonParallel")]
public class ScissorHandsApplicationTests
{
    [Theory]
    [InlineData("/", false)]
    [InlineData("/", true)]
    [InlineData("/docs/", false)]
    [InlineData("/docs/", true)]
    [InlineData("/manual/docs/", false)]
    [InlineData("/manual/docs/", true)]
    public async Task Given_BasePathAndLocale_When_PreviewRuns_Then_It_Should_ServePagesAssetsAndDirectoryRedirects(string baseUrl, bool useLocale)
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var originalDirectory = Directory.GetCurrentDirectory();
        var temporaryDirectory = Directory.CreateTempSubdirectory("scissorhands-preview-");
        try
        {
            Directory.SetCurrentDirectory(temporaryDirectory.FullName);
            var site = new SiteManifest { BaseUrl = baseUrl, Locale = "ko-KR", UseLocaleInUrl = useLocale };
            var route = useLocale ? "ko-kr/parent/child" : "parent/child";
            var postRoute = useLocale ? "ko-kr/2026/09/11/post" : "2026/09/11/post";
            var files = new Dictionary<string, string>
            {
                ["index.html"] = "home",
                [$"{route}/index.html"] = "child",
                [$"{postRoute}/index.html"] = "post",
                ["about & team/index.html"] = "encoded page",
                ["tags/index.html"] = "tags",
                ["tags/sample/index.html"] = "sample tag",
                ["404.html"] = "not found document",
                ["themes/default/assets/theme.css"] = "body { color: black; }",
                ["themes/default/assets/theme.js"] = "console.log('theme');",
                ["images/sample.svg"] = "<svg xmlns=\"http://www.w3.org/2000/svg\"></svg>"
            };
            var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var generator = Substitute.For<IStaticSiteGenerator>();
            generator.BuildAsync<MainLayout, IndexView, PostView, PageView, NotFoundView, TagListView, TagView>(
                Arg.Any<string>(), true, Arg.Any<CancellationToken>()).Returns(async call =>
                {
                    var destination = call.ArgAt<string>(0);
                    foreach (var file in files)
                    {
                        var filename = Path.Combine(destination, file.Key);
                        Directory.CreateDirectory(Path.GetDirectoryName(filename)!);
                        await File.WriteAllTextAsync(filename, file.Value, call.ArgAt<CancellationToken>(2));
                    }
                });

            var paths = Substitute.For<IAppPaths>();
            paths.GetContentsRoot().Returns(Path.Combine(temporaryDirectory.FullName, "contents"));
            paths.GetThemesRoot().Returns(Path.Combine(temporaryDirectory.FullName, "themes"));
            var watcherFactory = Substitute.For<IContentWatcherFactory>();
            watcherFactory.When(factory => factory.Create(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<Func<Task>>()))
                .Do(_ => ready.TrySetResult());

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { ContentRootPath = temporaryDirectory.FullName });
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Logging.ClearProviders();
            builder.Services.AddSingleton(site);
            builder.Services.AddSingleton(generator);
            builder.Services.AddSingleton(paths);
            builder.Services.AddSingleton(watcherFactory);
            var app = builder.Build();
            var components = new ThemeComponentSet(typeof(MainLayout), typeof(IndexView), typeof(PostView), typeof(PageView),
                typeof(NotFoundView), typeof(TagListView), typeof(TagView));
            var application = new ScissorHandsApplication(app, ["--preview"], components);
            var running = application.RunAsync();
            try
            {
                var started = await Task.WhenAny(ready.Task, running).WaitAsync(TimeSpan.FromSeconds(15), cancellationToken);
                await started;
                ready.Task.IsCompletedSuccessfully.ShouldBeTrue();
                var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
                using var handler = new HttpClientHandler { AllowAutoRedirect = false };
                using var client = new HttpClient(handler) { BaseAddress = new Uri(address), Timeout = TimeSpan.FromSeconds(10) };

                (await client.GetStringAsync(baseUrl, cancellationToken)).ShouldBe("home");
                (await client.GetStringAsync($"{baseUrl}{route}/", cancellationToken)).ShouldBe("child");
                (await client.GetStringAsync($"{baseUrl}{postRoute}/", cancellationToken)).ShouldBe("post");
                (await client.GetStringAsync($"{baseUrl}about%20%26%20team/", cancellationToken)).ShouldBe("encoded page");
                (await client.GetStringAsync($"{baseUrl}tags/", cancellationToken)).ShouldBe("tags");
                (await client.GetStringAsync($"{baseUrl}tags/sample/", cancellationToken)).ShouldBe("sample tag");
                (await client.GetStringAsync($"{baseUrl}404.html", cancellationToken)).ShouldBe("not found document");
                foreach (var asset in files.Where(file => file.Key.StartsWith("themes/", StringComparison.Ordinal) || file.Key.StartsWith("images/", StringComparison.Ordinal)))
                {
                    (await client.GetStringAsync($"{baseUrl}{asset.Key}", cancellationToken)).ShouldBe(asset.Value);
                }

                using var headRequest = new HttpRequestMessage(HttpMethod.Head, $"{baseUrl}themes/default/assets/theme.css");
                using var headResponse = await client.SendAsync(headRequest, cancellationToken);
                headResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
                headResponse.Content.Headers.ContentType!.MediaType.ShouldBe("text/css");
                (await headResponse.Content.ReadAsStringAsync(cancellationToken)).ShouldBeEmpty();

                using var redirect = await client.GetAsync($"{baseUrl}{route}?from=preview", cancellationToken);
                redirect.StatusCode.ShouldBe(HttpStatusCode.MovedPermanently);
                redirect.Headers.Location.ShouldBe(new Uri($"{address}{baseUrl}{route}/?from=preview"));
                (await client.GetStringAsync(redirect.Headers.Location, cancellationToken)).ShouldBe("child");

                using var missing = await client.GetAsync($"{baseUrl}missing/", cancellationToken);
                missing.StatusCode.ShouldBe(HttpStatusCode.NotFound);
                using var missingAsset = await client.GetAsync($"{baseUrl}themes/default/assets/missing.css", cancellationToken);
                missingAsset.StatusCode.ShouldBe(HttpStatusCode.NotFound);

                if (baseUrl != "/")
                {
                    using var mountRedirect = await client.GetAsync($"{baseUrl.TrimEnd('/')}?from=preview", cancellationToken);
                    mountRedirect.StatusCode.ShouldBe(HttpStatusCode.MovedPermanently);
                    mountRedirect.Headers.Location.ShouldBe(new Uri($"{address}{baseUrl}?from=preview"));
                    (await client.GetStringAsync($"/{route}/", cancellationToken)).ShouldBe("child");
                    using var nonmatchingPrefix = await client.GetAsync($"{baseUrl.TrimEnd('/')}-other/{route}/", cancellationToken);
                    nonmatchingPrefix.StatusCode.ShouldBe(HttpStatusCode.NotFound);
                    using var doubledPrefix = await client.GetAsync($"{baseUrl}{baseUrl.Trim('/')}/{route}/", cancellationToken);
                    doubledPrefix.StatusCode.ShouldBe(HttpStatusCode.NotFound);
                    Directory.Exists(Path.Combine(temporaryDirectory.FullName, "preview", baseUrl.Trim('/'))).ShouldBeFalse();
                }

                var previewDirectory = Path.Combine(temporaryDirectory.FullName, "preview");
                Directory.GetFiles(previewDirectory, "*", SearchOption.AllDirectories)
                    .Select(filename => Path.GetRelativePath(previewDirectory, filename).Replace('\\', '/'))
                    .Order(StringComparer.Ordinal).ShouldBe(files.Keys.Order(StringComparer.Ordinal));
            }
            finally
            {
                app.Lifetime.StopApplication();
                try
                {
                    await running.WaitAsync(TimeSpan.FromSeconds(15), CancellationToken.None);
                }
                finally
                {
                    await app.DisposeAsync();
                }
            }
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
            temporaryDirectory.Delete(recursive: true);
        }
    }
}
