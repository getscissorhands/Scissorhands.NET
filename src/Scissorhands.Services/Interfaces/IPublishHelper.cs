using System;
using System.Threading.Tasks;

using Aliencube.Scissorhands.Services.Helpers;

namespace Aliencube.Scissorhands.Services.Interfaces
{
    /// <summary>
    /// This provides interfaces to the <see cref="PublishHelper" /> class.
    /// </summary>
    public interface IPublishHelper : IDisposable
    {
        /// <summary>
        /// Reads the file
        /// </summary>
        /// <param name="filepath">
        /// The filepath.
        /// </param>
        /// <returns>
        /// Returns the contents.
        /// </returns>
        string Read(string filepath);

        /// <summary>
        /// Reads the file
        /// </summary>
        /// <param name="filepath">
        /// The filepath.
        /// </param>
        /// <returns>
        /// Returns the contents.
        /// </returns>
        Task<string> ReadAsync(string filepath);

        /// <summary>
        /// Creates the publish directory.
        /// </summary>
        /// <param name="publishDirectory">
        /// The publish directory.
        /// </param>
        void CreatePublishDirectory(string publishDirectory);

        /// <summary>
        /// Writes the contents to the designated path.
        /// </summary>
        /// <param name="contents">
        /// The contents.
        /// </param>
        /// <param name="publishpath">
        /// The publish path.
        /// </param>
        void Write(string contents, string publishpath);

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
        Task WriteAsync(string contents, string publishpath);
    }
}