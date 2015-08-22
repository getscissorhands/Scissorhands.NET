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
        private readonly Markdown _md;

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
        /// <param name="md">
        /// The <see cref="Markdown" /> instance.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Throws when the <c>options</c>, <c>engine</c> or <c>md</c> instance is null.
        /// </exception>
        public PublishService(ICommandOptions options, IRazorEngineService engine, Markdown md)
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

            if (md == null)
            {
                throw new ArgumentException("md");
            }

            this._md = md;
        }

        /// <summary>
        /// Processes posts.
        /// </summary>
        /// <returns>
        /// Returns <c>True</c>, if processed; otherwise returns <c>False</c>.
        /// </returns>
        public bool Process()
        {
            var post = this.GetPost();
            var model = this.GetModel<PageModel>(post);
            var template = this.GetTemplate();
            var compiled = this.Compile(template, model);
            var published = this.Publish(compiled);

            return published;
        }

        /// <summary>
        /// Processes posts.
        /// </summary>
        /// <returns>
        /// Returns <c>True</c>, if processed; otherwise returns <c>False</c>.
        /// </returns>
        public async Task<bool> ProcessAsync()
        {
            var post = await this.GetPostAsync();
            var model = this.GetModel<PageModel>(post);
            var template = await this.GetTemplateAsync();
            var compiled = await this.CompileAsync(template, model);
            var published = await this.PublishAsync(compiled);

            return published;
        }

        /// <summary>
        /// Gets the post from the given post file written in Markdown.
        /// </summary>
        /// <returns>
        /// Returns the HTML converted post.
        /// </returns>
        public string GetPost()
        {
            var postpath = Path.Combine(PostBasePath, this._options.Post);

            using (var stream = new FileStream(postpath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var doc = reader.ReadToEnd();
                var post = this._md.Transform(doc);
                return post;
            }
        }

        /// <summary>
        /// Gets the post from the given post file written in Markdown.
        /// </summary>
        /// <returns>
        /// Returns the HTML converted post.
        /// </returns>
        public async Task<string> GetPostAsync()
        {
            var postpath = Path.Combine(PostBasePath, this._options.Post);

            using (var stream = new FileStream(postpath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var doc = await reader.ReadToEndAsync();
                var post = this._md.Transform(doc);
                return post;
            }
        }

        /// <summary>
        /// Gets the page model for razor template.
        /// </summary>
        /// <param name="post">
        /// Post data.
        /// </param>
        /// <typeparam name="T">
        /// Type inheriting <see cref="BasePageModel" />.
        /// </typeparam>
        /// <returns>
        /// Returns the page model for razor template.
        /// </returns>
        public T GetModel<T>(string post) where T : BasePageModel
        {
            var model = Activator.CreateInstance<T>();
            model.Title = "Title";
            model.Author = "author";
            model.DateReleased = DateTime.UtcNow.ToLocalTime();
            model.Post = post;

            return model;
        }

        /// <summary>
        /// Gets the razor template.
        /// </summary>
        /// <returns>
        /// Returns the razor template.
        /// </returns>
        public string GetTemplate()
        {
            var filepath = Path.Combine(ThemeBasePath, this._options.Theme, "master.cshtml");

            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var template = reader.ReadToEnd();
                return template;
            }
        }

        /// <summary>
        /// Gets the razor template.
        /// </summary>
        /// <returns>
        /// Returns the razor template.
        /// </returns>
        public async Task<string> GetTemplateAsync()
        {
            var filepath = Path.Combine(ThemeBasePath, this._options.Theme, "master.cshtml");

            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var template = await reader.ReadToEndAsync();
                return template;
            }
        }

        /// <summary>
        /// Compiles post model with template.
        /// </summary>
        /// <param name="template">
        /// Template string.
        /// </param>
        /// <param name="model">
        /// The post model.
        /// </param>
        /// <typeparam name="T">
        /// Type inheriting the <see cref="BasePageModel" /> class.
        /// </typeparam>
        /// <returns>
        /// Returns the compiled string.
        /// </returns>
        public string Compile<T>(string template, T model) where T : BasePageModel
        {
            var compiled = this._engine.RunCompile(template, this._options.Theme, typeof(T), model);
            return compiled;
        }

        /// <summary>
        /// Compiles post model with template.
        /// </summary>
        /// <param name="template">
        /// Template string.
        /// </param>
        /// <param name="model">
        /// The post model.
        /// </param>
        /// <typeparam name="T">
        /// Type inheriting the <see cref="BasePageModel" /> class.
        /// </typeparam>
        /// <returns>
        /// Returns the compiled string.
        /// </returns>
        public async Task<string> CompileAsync<T>(string template, T model) where T : BasePageModel
        {
            string compiled = null;
            await Task.Run(() => { compiled = this._engine.RunCompile(template, this._options.Theme, typeof(T), model); });
            return compiled;
        }

        /// <summary>
        /// Publishes the blog posts.
        /// </summary>
        /// <param name="compiled">
        /// Template merged post data.
        /// </param>
        /// <returns>
        /// Returns <c>True</c>, if published; otherwise returns <c>False</c>.
        /// </returns>
        public bool Publish(string compiled)
        {
            if (!Directory.Exists(PublishedBasePath))
            {
                Directory.CreateDirectory(PublishedBasePath);
            }

            var publishpath = Path.Combine(PublishedBasePath, "date-released-" + this._options.Post.Replace(".md", ".html"));

            using (var stream = new FileStream(publishpath, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.Write(compiled);
            }

            return true;
        }

        /// <summary>
        /// Publishes the blog posts.
        /// </summary>
        /// <param name="compiled">
        /// Template merged post data.
        /// </param>
        /// <returns>
        /// Returns <c>True</c>, if published; otherwise returns <c>False</c>.
        /// </returns>
        public async Task<bool> PublishAsync(string compiled)
        {
            if (!Directory.Exists(PublishedBasePath))
            {
                Directory.CreateDirectory(PublishedBasePath);
            }

            var publishpath = Path.Combine(PublishedBasePath, "date-released-" + this._options.Post.Replace(".md", ".html"));

            using (var stream = new FileStream(publishpath, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                await writer.WriteAsync(compiled);
            }

            return true;
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