using System;
using System.Threading.Tasks;

namespace Aliencube.Scissorhands.Services.Interfaces
{
    /// <summary>
    /// This provides interfaces to the <see cref="PublishService" /> class.
    /// </summary>
    public interface IPublishService : IDisposable
    {
        /// <summary>
        /// Publishes the blog posts.
        /// </summary>
        /// <returns>
        /// Returns <c>True</c>, if published; otherwise returns <c>False</c>.
        /// </returns>
        bool Publish();

        /// <summary>
        /// Publishes the blog posts.
        /// </summary>
        /// <returns>
        /// Returns <c>True</c>, if published; otherwise returns <c>False</c>.
        /// </returns>
        Task<bool> PublishAsync();
    }
}