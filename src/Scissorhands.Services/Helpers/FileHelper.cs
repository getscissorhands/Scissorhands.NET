using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Aliencube.Scissorhands.Models;

using Microsoft.AspNet.Hosting;

namespace Aliencube.Scissorhands.Services.Helpers
{
    /// <summary>
    /// This represents the helper entity for files.
    /// </summary>
    public class FileHelper : IFileHelper
    {
        private readonly IHostingEnvironment _env;
        private readonly WebAppSettings _settings;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileHelper"/> class.
        /// </summary>
        /// <param name="env"><see cref="IHostingEnvironment"/> instance.</param>
        /// <param name="settings"><see cref="WebAppSettings"/> instance.</param>
        public FileHelper(IHostingEnvironment env, WebAppSettings settings)
        {
            if (env == null)
            {
                throw new ArgumentNullException(nameof(env));
            }

            this._env = env;

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            this._settings = settings;
        }

        /// <summary>
        /// Reads file and return the content as string.
        /// </summary>
        /// <param name="filepath">Fully qualified file path.</param>
        /// <returns>Returns the content as string.</returns>
        public async Task<string> ReadAsync(string filepath)
        {
            if (string.IsNullOrWhiteSpace(filepath))
            {
                return null;
            }

            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var contents = await reader.ReadToEndAsync().ConfigureAwait(false);
                return contents;
            }
        }

        /// <summary>
        /// Writes string content into a file.
        /// </summary>
        /// <param name="filepath">Fully qualified file path.</param>
        /// <param name="value">Content value.</param>
        /// <returns>Returns <c>True</c>; if the content has been written successfully; otherwise returns <c>False</c>.</returns>
        public async Task<bool> WriteAsync(string filepath, string value)
        {
            if (string.IsNullOrWhiteSpace(filepath))
            {
                throw new ArgumentNullException(nameof(filepath));
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                return await Task.FromResult(false).ConfigureAwait(false);
            }

            using (var stream = new FileStream(filepath, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                await writer.WriteAsync(value).ConfigureAwait(false);
                return await Task.FromResult(true).ConfigureAwait(false);
            }
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