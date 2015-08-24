using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Aliencube.Scissorhands.Services.Configs;
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
        private readonly IYamlSettings _settings;
        private readonly ICommandOptions _options;
        private readonly IRazorEngineService _engine;
        private readonly Markdown _md;

        private string _themeBasePath;
        private string _postBasePath;
        private string _publishedBasePath;

        private bool _disposed;

        /// <summary>
        /// Initialises a new instance of the <see cref="PublishService"/> class.
        /// </summary>
        /// <param name="settings">
        /// The <see cref="YamlSettings" /> instance.
        /// </param>
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
        public PublishService(IYamlSettings settings, ICommandOptions options, IRazorEngineService engine, Markdown md)
        {
            if (settings == null)
            {
                throw new ArgumentException("settings");
            }

            this._settings = settings;

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

            this.SetBasePaths();
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
            var postpath = Path.Combine(this._postBasePath, this._options.Post);

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
            var postpath = Path.Combine(this._postBasePath, this._options.Post);

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
            var filepath = Path.Combine(this._themeBasePath, this._options.Theme, "master.cshtml");

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
            var filepath = Path.Combine(this._themeBasePath, this._options.Theme, "master.cshtml");

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
            if (!Directory.Exists(this._publishedBasePath))
            {
                Directory.CreateDirectory(this._publishedBasePath);
            }

            var publishpath = Path.Combine(this._publishedBasePath, "date-released-" + this._options.Post.Replace(".md", ".html"));

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
            if (!Directory.Exists(this._publishedBasePath))
            {
                Directory.CreateDirectory(this._publishedBasePath);
            }

            var publishpath = Path.Combine(this._publishedBasePath, "date-released-" + this._options.Post.Replace(".md", ".html"));

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

        private void SetBasePaths()
        {
            this._themeBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this._settings.Directories.Themes);
            this._postBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this._settings.Directories.Posts);
            this._publishedBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this._settings.Directories.Published);
        }
    }
}