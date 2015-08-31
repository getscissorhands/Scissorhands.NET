using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Aliencube.Scissorhands.Services.Configs;
using Aliencube.Scissorhands.Services.Interfaces;
using Aliencube.Scissorhands.Services.Models;

using RazorEngine.Templating;

namespace Aliencube.Scissorhands.Services.Helpers
{
    /// <summary>
    /// This represents the helper entity for publish posts.
    /// </summary>
    public class PublishHelper : IPublishHelper
    {
        private readonly IYamlSettings _settings;
        private readonly IRazorEngineService _engine;

        private bool _disposed;

        /// <summary>
        /// Initialises a new instance of the <see cref="PublishHelper" /> class.
        /// </summary>
        /// <param name="settings">
        /// The <see cref="YamlSettings" /> instance.
        /// </param>
        /// <param name="engine">
        /// The <see cref="IRazorEngineService" /> instance.
        /// </param>
        public PublishHelper(IYamlSettings settings, IRazorEngineService engine)
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
        }

        /// <summary>
        /// Reads the file
        /// </summary>
        /// <param name="filepath">
        /// The filepath.
        /// </param>
        /// <returns>
        /// Returns the contents.
        /// </returns>
        public string Read(string filepath)
        {
            if (string.IsNullOrWhiteSpace(filepath))
            {
                return null;
            }

            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var contents = reader.ReadToEnd();
                return contents;
            }
        }

        /// <summary>
        /// Reads the file
        /// </summary>
        /// <param name="filepath">
        /// The filepath.
        /// </param>
        /// <returns>
        /// Returns the contents.
        /// </returns>
        public async Task<string> ReadAsync(string filepath)
        {
            if (string.IsNullOrWhiteSpace(filepath))
            {
                return null;
            }

            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var contents = await reader.ReadToEndAsync();
                return contents;
            }
        }

        /// <summary>
        /// Creates the publish directory.
        /// </summary>
        /// <param name="publishDirectory">
        /// The publish directory.
        /// </param>
        public void CreatePublishDirectory(string publishDirectory)
        {
            if (string.IsNullOrWhiteSpace(publishDirectory))
            {
                throw new ArgumentNullException("publishDirectory");
            }

            if (Directory.Exists(publishDirectory))
            {
                return;
            }

            Directory.CreateDirectory(publishDirectory);
        }

        /// <summary>
        /// Writes the content to the designated path.
        /// </summary>
        /// <param name="publishpath">
        /// The publish path.
        /// </param>
        /// <param name="content">
        /// The content.
        /// </param>
        public void Write(string publishpath, string content)
        {
            if (string.IsNullOrWhiteSpace(publishpath))
            {
                throw new ArgumentNullException("publishpath");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return;
            }

            using (var stream = new FileStream(publishpath, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.Write(content);
            }
        }

        /// <summary>
        /// Writes the content to the designated path.
        /// </summary>
        /// <param name="publishpath">
        /// The publish path.
        /// </param>
        /// <param name="content">
        /// The content.
        /// </param>
        /// <returns>
        /// Returns the <see cref="Task" />.
        /// </returns>
        public async Task WriteAsync(string publishpath, string content)
        {
            if (string.IsNullOrWhiteSpace(publishpath))
            {
                throw new ArgumentNullException("publishpath");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return;
            }

            using (var stream = new FileStream(publishpath, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                await writer.WriteAsync(content);
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
            if (string.IsNullOrWhiteSpace(template))
            {
                throw new ArgumentNullException("template");
            }

            if (model == null)
            {
                throw new ArgumentNullException("model");
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
                throw new ArgumentNullException("template");
            }

            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            string compiled = null;
            await Task.Run(() => { compiled = this.Compile(template, model); });
            return compiled;
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