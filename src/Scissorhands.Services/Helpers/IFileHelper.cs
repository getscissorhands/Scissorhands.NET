using System;
using System.Threading.Tasks;

namespace Aliencube.Scissorhands.Services.Helpers
{
    /// <summary>
    /// This provides interfaces to the <see cref="FileHelper"/> class.
    /// </summary>
    public interface IFileHelper : IDisposable
    {
        /// <summary>
        /// Reads file and returns the content as string.
        /// </summary>
        /// <param name="filepath">Fully qualified file path.</param>
        /// <returns>Returns the content as string.</returns>
        Task<string> ReadAsync(string filepath);

        /// <summary>
        /// Writes string content into a file.
        /// </summary>
        /// <param name="filepath">Fully qualified file path.</param>
        /// <param name="value">Content value.</param>
        /// <returns>Returns <c>True</c>; if the content has been written successfully; otherwise returns <c>False</c>.</returns>
        Task<bool> WriteAsync(string filepath, string value);
    }
}