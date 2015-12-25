using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Aliencube.Scissorhands.Services.Helpers
{
    /// <summary>
    /// This represents the helper entity for files.
    /// </summary>
    public class FileHelper : IFileHelper
    {
        private bool _disposed;

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