using System;
using System.Threading.Tasks;

using Aliencube.Scissorhands.ConsoleApp.Interfaces;

namespace Aliencube.Scissorhands.ConsoleApp
{
    /// <summary>
    /// This represents the service entity for publishing.
    /// </summary>
    public class PublishService : IPublishService
    {
        private readonly ICommandOptions _options;

        private bool _disposed;

        /// <summary>
        /// Initialises a new instance of the <see cref="PublishService"/> class.
        /// </summary>
        /// <param name="options">
        /// The <see cref="CommandOptions" /> instance.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Throws when the <c>options</c> instance is null.
        /// </exception>
        public PublishService(ICommandOptions options)
        {
            if (options == null)
            {
                throw new ArgumentException("options");
            }

            this._options = options;
        }

        /// <summary>
        /// Publishes the blog posts.
        /// </summary>
        /// <returns>
        /// Returns <c>True</c>, if published; otherwise returns <c>False</c>.
        /// </returns>
        public bool Publish()
        {
            return true;
        }

        /// <summary>
        /// Publishes the blog posts.
        /// </summary>
        /// <returns>
        /// Returns <c>True</c>, if published; otherwise returns <c>False</c>.
        /// </returns>
        public async Task<bool> PublishAsync()
        {
            return await Task.FromResult(true);
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