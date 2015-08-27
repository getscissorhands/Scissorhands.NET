using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Aliencube.Scissorhands.Services.Interfaces;

namespace Aliencube.Scissorhands.Services.Helpers
{
    /// <summary>
    /// This represents the helper entity for publish posts.
    /// </summary>
    public class PublishHelper : IPublishHelper
    {
        private bool _disposed;

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
        /// Writes the contents to the designated path.
        /// </summary>
        /// <param name="contents">
        /// The contents.
        /// </param>
        /// <param name="publishpath">
        /// The publish path.
        /// </param>
        public void Write(string contents, string publishpath)
        {
            if (string.IsNullOrWhiteSpace(contents))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(publishpath))
            {
                throw new ArgumentNullException("publishpath");
            }

            using (var stream = new FileStream(publishpath, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.Write(contents);
            }
        }

        /// <summary>
        /// Writes the contents to the designated path.
        /// </summary>
        /// <param name="contents">
        /// The contents.
        /// </param>
        /// <param name="publishpath">
        /// The publish path.
        /// </param>
        /// <returns>
        /// Returns the <see cref="Task" />.
        /// </returns>
        public async Task WriteAsync(string contents, string publishpath)
        {
            if (string.IsNullOrWhiteSpace(contents))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(publishpath))
            {
                throw new ArgumentNullException("publishpath");
            }

            using (var stream = new FileStream(publishpath, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                await writer.WriteAsync(contents);
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