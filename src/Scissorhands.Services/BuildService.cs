using System;
using System.Threading.Tasks;

using Scissorhands.Helpers;

namespace Scissorhands.Services
{
    /// <summary>
    /// This represents the service entity for build.
    /// </summary>
    public class BuildService : IBuildService
    {
        private readonly IFileHelper _fileHelper;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildService"/> class.
        /// </summary>
        /// <param name="fileHelper"><see cref="IFileHelper"/> instance.</param>
        public BuildService(IFileHelper fileHelper)
        {
            if (fileHelper == null)
            {
                throw new ArgumentNullException(nameof(fileHelper));
            }

            this._fileHelper = fileHelper;
        }

        /// <summary>
        /// Builds the entire blog.
        /// </summary>
        /// <returns>Returns the <see cref="Task"/>.</returns>
        public async Task BuildAsync()
        {
            await this.BuildIndiciesAsync().ConfigureAwait(false);
            await this.BuildPostsAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Builds all index pages including pagination.
        /// </summary>
        /// <returns>Returns the <see cref="Task"/>.</returns>
        public Task<string> BuildIndiciesAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds all posts.
        /// </summary>
        /// <returns>Returns the <see cref="Task"/>.</returns>
        public Task<string> BuildPostsAsync()
        {
            throw new NotImplementedException();
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