using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Aliencube.Scissorhands.Services.Configs;
using Aliencube.Scissorhands.Services.Helpers;
using Aliencube.Scissorhands.Services.Models;

using MarkdownDeep;

namespace Aliencube.Scissorhands.Services.Processors
{
    /// <summary>
    /// This represents the helper entity for posts.
    /// </summary>
    public class PostProcessor : BaseProcessor, IPostProcessor
    {
        private readonly IYamlSettings _settings;
        private readonly Markdown _md;
        private readonly IPublishHelper _helper;

        /// <summary>
        /// Initialises a new instance of the <see cref="PostProcessor" /> class.
        /// </summary>
        /// <param name="settings">
        /// The <see cref="YamlSettings" /> instance.
        /// </param>
        /// <param name="md">
        /// The <see cref="Markdown" /> instance.
        /// </param>
        /// <param name="helper">
        /// The <see cref="PublishHelper" /> instance.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Throws when the <c>options</c>, <c>engine</c> or <c>md</c> instance is null.
        /// </exception>
        public PostProcessor(IYamlSettings settings, Markdown md, IPublishHelper helper)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            this._settings = settings;

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

            var filepath = Path.Combine(this._settings.ThemeBasePath, theme.Name, theme.Master);
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

            var filepath = Path.Combine(this._settings.ThemeBasePath, theme.Name, theme.Master);
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
                throw new ArgumentNullException("postpath");
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
                throw new ArgumentNullException("postpath");
            }

            var doc = await this._helper.ReadAsync(postpath);
            var post = this._md.Transform(doc);
            return post;
        }

        /// <summary>
        /// Gets the list of post paths.
        /// </summary>
        /// <param name="postpath">
        /// The postpath for a single post.
        /// </param>
        /// <returns>
        /// Returns the list of post paths.
        /// </returns>
        public IEnumerable<string> GetPostPaths(string postpath = null)
        {
            IEnumerable<string> paths = new List<string>();
            if (string.IsNullOrWhiteSpace(postpath))
            {
                paths = Directory.GetFiles(this._settings.Directories.Posts)
                                 .Where(filepath => IsMarkdownPost(filepath, this._settings.Contents.Extension));
            }
            else
            {
                var path = Path.Combine(this._settings.PostBasePath, postpath);
                if (IsSinglePost(path, this._settings.Contents.Extension))
                {
                    paths = new List<string>() { path };
                }
            }

            return paths;
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
                throw new ArgumentNullException("post");
            }

            var model = Activator.CreateInstance<T>();
            model.Title = "Title";
            model.Author = "author";
            model.DateReleased = DateTime.UtcNow.ToLocalTime();
            model.Post = post;

            return model;
        }

        /// <summary>
        /// Processes the blog posts.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// Returns <c>True</c>, if processed; otherwise returns <c>False</c>.
        /// </returns>
        public override bool Process(ProcessContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (!context.Items.ContainsKey("postpath"))
            {
                throw new ContextNotFoundException("postpath");
            }

            var postpath = (string)context.Items["postpath"];

            if (!context.Items.ContainsKey("content"))
            {
                throw new ContextNotFoundException("postpath");
            }

            var content = (string)context.Items["content"];

            this._helper.CreatePublishDirectory(this._settings.PublishedBasePath);

            var publishpath = Path.Combine(this._settings.PublishedBasePath, "date-released-" + postpath.Replace(this._settings.Contents.Extension, ".html"));

            this._helper.Write(publishpath, content);

            return true;
        }

        /// <summary>
        /// Processes the blog posts.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// Returns <c>True</c>, if processed; otherwise returns <c>False</c>.
        /// </returns>
        public override async Task<bool> ProcessAsync(ProcessContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (!context.Items.ContainsKey("postpath"))
            {
                throw new ContextNotFoundException("postpath");
            }

            var postpath = (string)context.Items["postpath"];

            if (!context.Items.ContainsKey("content"))
            {
                throw new ContextNotFoundException("postpath");
            }

            var content = (string)context.Items["content"];

            this._helper.CreatePublishDirectory(this._settings.PublishedBasePath);

            var publishpath = Path.Combine(this._settings.PublishedBasePath, "date-released-" + postpath.Replace(this._settings.Contents.Extension, ".html"));

            await this._helper.WriteAsync(publishpath, content);

            return true;
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
    }
}