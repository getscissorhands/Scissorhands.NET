using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Aliencube.Scissorhands.Services.Configs;
using Aliencube.Scissorhands.Services.Helpers;
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
        private readonly IRazorEngineService _engine;
        private readonly Markdown _md;
        private readonly IPublishHelper _helper;

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
        /// <param name="engine">
        /// The <see cref="RazorEngineService" /> instance.
        /// </param>
        /// <param name="md">
        /// The <see cref="Markdown" /> instance.
        /// </param>
        /// <param name="helper">
        /// The <see cref="PublishHelper" /> instance.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Throws when the <c>options</c>, <c>engine</c> or <c>md</c> instance is null.
        /// </exception>
        public PublishService(IYamlSettings settings, IRazorEngineService engine, Markdown md, IPublishHelper helper)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            this._settings = settings;

            if (engine == null)
            {
                throw new ArgumentNullException("engine");
            }

            this._engine = engine;

            if (md == null)
            {
                throw new ArgumentNullException("md");
            }

            this._md = md;

            if (helper == null)
            {
                throw new ArgumentNullException("helper");
            }

            this._helper = helper;

            this.SetBasePaths();
        }

        /// <summary>
        /// Processes posts.
        /// </summary>
        /// <param name="postpath">
        /// The filename of the post to process.
        /// </param>
        /// <returns>
        /// Returns <c>True</c>, if processed; otherwise returns <c>False</c>.
        /// </returns>
        public bool Process(string postpath = null)
        {
            var template = this.GetTemplate(this._settings.Contents.Theme);
            var paths = this.GetPostPaths(postpath);

            foreach (var path in paths)
            {
                var post = this.GetPost(path);
                var model = this.GetModel<PageModel>(post);
                var compiled = this.Compile(template, model);
                var published = this.Publish(path, compiled);

                if (!published)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Processes posts.
        /// </summary>
        /// <param name="postpath">
        /// The filename of the post to process.
        /// </param>
        /// <returns>
        /// Returns <c>True</c>, if processed; otherwise returns <c>False</c>.
        /// </returns>
        public async Task<bool> ProcessAsync(string postpath = null)
        {
            var template = await this.GetTemplateAsync(this._settings.Contents.Theme);
            var paths = this.GetPostPaths(postpath);

            foreach (var path in paths)
            {
                var post = await this.GetPostAsync(path);
                var model = this.GetModel<PageModel>(post);
                var compiled = await this.CompileAsync(template, model);
                var published = await this.PublishAsync(path, compiled);

                if (!published)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the razor template.
        /// </summary>
        /// <param name="themeName">
        /// The theme name.
        /// </param>
        /// <returns>
        /// Returns the razor template.
        /// </returns>
        public string GetTemplate(string themeName)
        {
            var name = "default";
            if (!string.IsNullOrWhiteSpace(themeName))
            {
                name = themeName;
            }

            var theme =
                this._settings.Themes.SingleOrDefault(
                    p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                ?? this._settings.Themes.Single(
                    p => p.Name.Equals("default", StringComparison.InvariantCultureIgnoreCase));

            var filepath = Path.Combine(this._themeBasePath, theme.Name, theme.Master);
            var template = this._helper.Read(filepath);
            return template;
        }

        /// <summary>
        /// Gets the razor template.
        /// </summary>
        /// <param name="themeName">
        /// The theme name.
        /// </param>
        /// <returns>
        /// Returns the razor template.
        /// </returns>
        public async Task<string> GetTemplateAsync(string themeName)
        {
            var name = "default";
            if (!string.IsNullOrWhiteSpace(themeName))
            {
                name = themeName;
            }

            var theme =
                this._settings.Themes.SingleOrDefault(
                    p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                ?? this._settings.Themes.Single(
                    p => p.Name.Equals("default", StringComparison.InvariantCultureIgnoreCase));

            var filepath = Path.Combine(this._themeBasePath, theme.Name, theme.Master);
            var template = await this._helper.ReadAsync(filepath);
            return template;
        }

        /// <summary>
        /// Gets the post from the given post file written in Markdown.
        /// </summary>
        /// <param name="postpath">
        /// The full path of the post.
        /// </param>
        /// <returns>
        /// Returns the HTML converted post.
        /// </returns>
        public string GetPost(string postpath)
        {
            if (string.IsNullOrWhiteSpace(postpath))
            {
                throw new ArgumentException("postpath");
            }

            var doc = this._helper.Read(postpath);
            var post = this._md.Transform(doc);
            return post;
        }

        /// <summary>
        /// Gets the post from the given post file written in Markdown.
        /// </summary>
        /// <param name="postpath">
        /// The full path of the post.
        /// </param>
        /// <returns>
        /// Returns the HTML converted post.
        /// </returns>
        public async Task<string> GetPostAsync(string postpath)
        {
            if (string.IsNullOrWhiteSpace(postpath))
            {
                throw new ArgumentException("postpath");
            }

            var doc = await this._helper.ReadAsync(postpath);
            var post = this._md.Transform(doc);
            return post;
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
            if (string.IsNullOrWhiteSpace(post))
            {
                throw new ArgumentException("post");
            }

            var model = Activator.CreateInstance<T>();
            model.Title = "Title";
            model.Author = "author";
            model.DateReleased = DateTime.UtcNow.ToLocalTime();
            model.Post = post;

            return model;
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
            if (string.IsNullOrWhiteSpace(template))
            {
                throw new ArgumentException("template");
            }

            if (model == null)
            {
                throw new ArgumentException("model");
            }

            var compiled = this._engine.RunCompile(template, this._settings.Contents.Theme, typeof(T), model);
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
            if (string.IsNullOrWhiteSpace(template))
            {
                throw new ArgumentException("template");
            }

            if (model == null)
            {
                throw new ArgumentException("model");
            }

            string compiled = null;
            await Task.Run(() => { compiled = this.Compile(template, model); });
            return compiled;
        }

        /// <summary>
        /// Publishes the blog posts.
        /// </summary>
        /// <param name="postpath">
        /// The full path of the post.
        /// </param>
        /// <param name="compiled">
        /// Template merged post data.
        /// </param>
        /// <returns>
        /// Returns <c>True</c>, if published; otherwise returns <c>False</c>.
        /// </returns>
        public bool Publish(string postpath, string compiled)
        {
            if (string.IsNullOrWhiteSpace(compiled))
            {
                throw new ArgumentException("postpath");
            }

            if (string.IsNullOrWhiteSpace(compiled))
            {
                throw new ArgumentException("compiled");
            }

            this._helper.CreatePublishDirectory(this._publishedBasePath);

            var publishpath = Path.Combine(this._publishedBasePath, "date-released-" + postpath.Replace(this._settings.Contents.Extension, ".html"));

            this._helper.Write(compiled, publishpath);

            return true;
        }

        /// <summary>
        /// Publishes the blog posts.
        /// </summary>
        /// <param name="postpath">
        /// The full path of the post.
        /// </param>
        /// <param name="compiled">
        /// Template merged post data.
        /// </param>
        /// <returns>
        /// Returns <c>True</c>, if published; otherwise returns <c>False</c>.
        /// </returns>
        public async Task<bool> PublishAsync(string postpath, string compiled)
        {
            if (string.IsNullOrWhiteSpace(compiled))
            {
                throw new ArgumentException("postpath");
            }

            if (string.IsNullOrWhiteSpace(compiled))
            {
                throw new ArgumentException("compiled");
            }

            this._helper.CreatePublishDirectory(this._publishedBasePath);

            var publishpath = Path.Combine(this._publishedBasePath, "date-released-" + postpath.Replace(this._settings.Contents.Extension, ".html"));

            await this._helper.WriteAsync(compiled, publishpath);

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

        private static bool IsSinglePost(string postpath, string extension)
        {
            if (string.IsNullOrWhiteSpace(postpath))
            {
                return false;
            }

            var result = IsMarkdownPost(postpath, extension);
            return result;
        }

        private static bool IsMarkdownPost(string postpath, string extension)
        {
            if (string.IsNullOrWhiteSpace(postpath))
            {
                throw new ArgumentNullException("postpath");
            }

            if (string.IsNullOrWhiteSpace(extension))
            {
                throw new ArgumentNullException("extension");
            }

            var result = postpath.ToLowerInvariant().EndsWith(extension.ToLowerInvariant());
            return result;
        }

        private void SetBasePaths()
        {
            this._themeBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this._settings.Directories.Themes);
            this._postBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this._settings.Directories.Posts);
            this._publishedBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this._settings.Directories.Published);
        }

        private IEnumerable<string> GetPostPaths(string postpath = null)
        {
            IEnumerable<string> paths = new List<string>();
            if (string.IsNullOrWhiteSpace(postpath))
            {
                paths = Directory.GetFiles(this._settings.Directories.Posts)
                                 .Where(filepath => IsMarkdownPost(filepath, this._settings.Contents.Extension));
            }
            else
            {
                var path = Path.Combine(this._postBasePath, postpath);
                if (IsSinglePost(path, this._settings.Contents.Extension))
                {
                    paths = new List<string>() { path };
                }
            }

            return paths;
        }
    }
}