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
        /// Reads file and return the content as string.
        /// </summary>
        /// <param name="filepath">Fully qualified file path.</param>
        /// <returns>Returns the content as string.</returns>
        Task<string> ReadAsync(string filepath);
    }
}