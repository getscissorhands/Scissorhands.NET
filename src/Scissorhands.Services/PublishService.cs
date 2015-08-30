using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Aliencube.Scissorhands.Services.Configs;
using Aliencube.Scissorhands.Services.Helpers;
using Aliencube.Scissorhands.Services.Interfaces;
using Aliencube.Scissorhands.Services.Models;

namespace Aliencube.Scissorhands.Services
{
    /// <summary>
    /// This represents the service entity for publishing.
    /// </summary>
    public class PublishService : IPublishService
    {
        private readonly IYamlSettings _settings;
        private readonly IPostHelper _postHelper;
        private readonly IPublishHelper _publishHelper;

        private bool _disposed;

        /// <summary>
        /// Initialises a new instance of the <see cref="PublishService" /> class.
        /// </summary>
        /// <param name="settings">
        /// The <see cref="YamlSettings" /> instance.
        /// </param>
        /// <param name="postHelper">
        /// The <see cref="PostHelper" /> instance.
        /// </param>
        /// <param name="publishHelper">
        /// The <see cref="PublishHelper" /> instance.
        /// </param>
        public PublishService(IYamlSettings settings, IPostHelper postHelper, IPublishHelper publishHelper)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            this._settings = settings;

            if (postHelper == null)
            {
                throw new ArgumentNullException("postHelper");
            }

            this._postHelper = postHelper;

            if (publishHelper == null)
            {
                throw new ArgumentNullException("publishHelper");
            }

            this._publishHelper = publishHelper;

            this.PublishResults = new Dictionary<string, bool>();
        }

        /// <summary>
        /// Gets the list of publish results of posts.
        /// </summary>
        public IDictionary<string, bool> PublishResults { get; private set; }

        /// <summary>
        /// Publishes either a single post or entire blog posts.
        /// </summary>
        /// <param name="postpath">
        /// The post path written in Markdown.
        /// </param>
        public void Publish(string postpath = null)
        {
            var template = this._postHelper.GetTemplate(this._settings.Contents.Theme);
            var paths = this._postHelper.GetPostPaths(postpath);

            foreach (var path in paths)
            {
                var post = this._postHelper.GetPost(path);
                var model = this._postHelper.GetModel<PageModel>(post);
                var compiled = this._publishHelper.Compile(template, model);
                var published = this.Process(path, compiled);

                this.SetPublishResult(path, published);
            }
        }

        /// <summary>
        /// Publishes either a single post or entire blog posts.
        /// </summary>
        /// <param name="postpath">
        /// The post path written in Markdown.
        /// </param>
        /// <returns>
        /// Returns the <see cref="Task" />.
        /// </returns>
        public async Task PublishAsync(string postpath = null)
        {
            var template = await this._postHelper.GetTemplateAsync(this._settings.Contents.Theme);
            var paths = this._postHelper.GetPostPaths(postpath);

            foreach (var path in paths)
            {
                var post = await this._postHelper.GetPostAsync(path);
                var model = this._postHelper.GetModel<PageModel>(post);
                var compiled = await this._publishHelper.CompileAsync(template, model);
                var published = await this.ProcessAsync(path, compiled);

                this.SetPublishResult(path, published);
            }
        }

        /// <summary>
        /// Processes the blog posts.
        /// </summary>
        /// <param name="postpath">
        /// The full path of the post written in Markdown.
        /// </param>
        /// <param name="content">
        /// Blog content converted in HTML.
        /// </param>
        /// <returns>
        /// Returns <c>True</c>, if processed; otherwise returns <c>False</c>.
        /// </returns>
        public bool Process(string postpath, string content)
        {
            if (string.IsNullOrWhiteSpace(postpath))
            {
                throw new ArgumentNullException("postpath");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentNullException("content");
            }

            this._publishHelper.CreatePublishDirectory(this._settings.PublishedBasePath);

            var publishpath = Path.Combine(this._settings.PublishedBasePath, "date-released-" + postpath.Replace(this._settings.Contents.Extension, ".html"));

            this._publishHelper.Write(publishpath, content);

            return true;
        }

        /// <summary>
        /// Processes the blog posts.
        /// </summary>
        /// <param name="postpath">
        /// The full path of the post written in Markdown.
        /// </param>
        /// <param name="content">
        /// Blog content converted in HTML.
        /// </param>
        /// <returns>
        /// Returns <c>True</c>, if processed; otherwise returns <c>False</c>.
        /// </returns>
        public async Task<bool> ProcessAsync(string postpath, string content)
        {
            if (string.IsNullOrWhiteSpace(postpath))
            {
                throw new ArgumentNullException("postpath");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentNullException("content");
            }

            this._publishHelper.CreatePublishDirectory(this._settings.PublishedBasePath);

            var publishpath = Path.Combine(this._settings.PublishedBasePath, "date-released-" + postpath.Replace(this._settings.Contents.Extension, ".html"));

            await this._publishHelper.WriteAsync(publishpath, content);

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

        private void SetPublishResult(string postpath, bool published)
        {
            if (this.PublishResults.ContainsKey(postpath))
            {
                this.PublishResults.Remove(postpath);
            }

            this.PublishResults.Add(postpath, published);
        }
    }
}