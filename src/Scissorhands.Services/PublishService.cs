using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Aliencube.Scissorhands.Services.Interfaces;
using Aliencube.Scissorhands.Services.Models;

using MarkdownDeep;

using RazorEngine.Templating;

namespace Aliencube.Scissorhands.Services
{
    /// <summary>
    /// This represents the service entity for publishing.
    /// </summary>
    public class PublishService : IPublishService
    {
        private static readonly string ThemeBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Themes");
        private static readonly string PostBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Posts");
        private static readonly string PublishedBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Published");

        private readonly ICommandOptions _options;
        private readonly IRazorEngineService _engine;

        private bool _disposed;

        /// <summary>
        /// Initialises a new instance of the <see cref="PublishService"/> class.
        /// </summary>
        /// <param name="options">
        /// The <see cref="CommandOptions" /> instance.
        /// </param>
        /// <param name="engine">
        /// The <see cref="RazorEngineService" /> instance.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Throws when the <c>options</c> instance is null.
        /// </exception>
        public PublishService(ICommandOptions options, IRazorEngineService engine)
        {
            if (options == null)
            {
                throw new ArgumentException("options");
            }

            this._options = options;

            if (engine == null)
            {
                throw new ArgumentException("engine");
            }

            this._engine = engine;
        }

        /// <summary>
        /// Publishes the blog posts.
        /// </summary>
        /// <returns>
        /// Returns <c>True</c>, if published; otherwise returns <c>False</c>.
        /// </returns>
        public bool Publish()
        {
            // Gets post in Markdown and converts it to HTML.
            var postpath = Path.Combine(PostBasePath, this._options.Theme, this._options.Post);
            string parsed;
            using (var stream = new FileStream(postpath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var md = new Markdown() { ExtraMode = true, SafeMode = false };
                parsed = md.Transform(reader.ReadToEnd());
            }

            // Sets page model.
            var model = new PageModel();
            model.Title = "Title";
            model.Author = "author";
            model.DateReleased = DateTime.UtcNow.ToLocalTime();
            model.Post = parsed;

            // Merge template.
            var filepath = Path.Combine(ThemeBasePath, this._options.Theme, "master.cshtml");
            string result;
            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var template = reader.ReadToEnd();
                result = this._engine.RunCompile(template, this._options.Theme, typeof(PageModel), model);
            }

            // Publish post.
            var publishpath = Path.Combine(PublishedBasePath, "date-released-" + this._options.Post.Replace(".md", ".html"));
            using (var stream = new FileStream(publishpath, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.Write(result);
            }

            return true;
        }

        /// <summary>
        /// Publishes the blog posts.
        /// </summary>
        /// <returns>
        /// Returns <c>True</c>, if published; otherwise returns <c>False</c>.
        /// </returns>
        public async Task<bool> PublishAsync()
        {
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this._disposed = true;
        }
    }
}