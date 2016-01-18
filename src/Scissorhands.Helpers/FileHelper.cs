using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.PlatformAbstractions;

using Scissorhands.Helpers.Extensions;
using Scissorhands.Models.Settings;

namespace Scissorhands.Helpers
{
    /// <summary>
    /// This represents the helper entity for files.
    /// </summary>
    public class FileHelper : IFileHelper
    {
        private readonly WebAppSettings _settings;
        private readonly IApplicationEnvironment _appEnv;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileHelper"/> class.
        /// </summary>
        /// <param name="settings"><see cref="WebAppSettings"/> instance.</param>
        public FileHelper(WebAppSettings settings, IApplicationEnvironment appEnv)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            this._settings = settings;

            if (appEnv == null)
            {
                throw new ArgumentNullException(nameof(appEnv));
            }

            this._appEnv = appEnv;
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

            using (var stream = new FileStream(ReplaceDirectorySeparator(filepath), FileMode.Open, FileAccess.Read))
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

            using (var stream = new FileStream(ReplaceDirectorySeparator(filepath), FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                await writer.WriteAsync(value).ConfigureAwait(false);
                return await Task.FromResult(true).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Checks whether the directory path exists or not.
        /// </summary>
        /// <param name="directorypath">Directory path.</param>
        /// <returns>Returns the fully qualified directory path.</returns>
        public string GetDirectory(string directorypath)
        {
            if (string.IsNullOrWhiteSpace(directorypath))
            {
                return null;
            }

            var trimmedBasePath = ReplaceDirectorySeparator(this._appEnv.ApplicationBasePath);
            var trimmedDirectoryPath = TrimDirectoryPath(directorypath);

            var combined =
                Path.Combine(
                    new[]
                        {
                            trimmedBasePath,
                            "wwwroot",
                            trimmedDirectoryPath,
                        });

            if (!Directory.Exists(combined))
            {
                Directory.CreateDirectory(combined);
            }

            return combined;
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

        private static string ReplaceDirectorySeparator(string directorypath)
        {
            var replaced = directorypath.Replace('/', Path.DirectorySeparatorChar);
            return replaced;
        }

        private static string TrimDirectoryPath(string directorypath)
        {
            var path = ReplaceDirectorySeparator(directorypath);
            if (path.StartsWith(Path.DirectorySeparatorChar))
            {
                path = path.Substring(1);
            }

            return path;
        }
    }
}