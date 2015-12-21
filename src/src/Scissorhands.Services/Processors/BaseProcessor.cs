using System.Threading.Tasks;

namespace Aliencube.Scissorhands.Services.Processors
{
    /// <summary>
    /// This represents the abstract entity for processor. This must be inherited.
    /// </summary>
    public abstract class BaseProcessor : IProcessor
    {
        private bool _disposed;

        /// <summary>
        /// Processes the given context.
        /// </summary>
        /// <param name="context">
        /// The <see cref="ProcessContext" /> instance.
        /// </param>
        /// <returns>
        /// Returns <c>True</c>, if processed; otherwise returns <c>False</c>.
        /// </returns>
        public abstract bool Process(ProcessContext context);

        /// <summary>
        /// Processes the given context.
        /// </summary>
        /// <param name="context">
        /// The <see cref="ProcessContext" /> instance.
        /// </param>
        /// <returns>
        /// Returns <c>True</c>, if processed; otherwise returns <c>False</c>.
        /// </returns>
        public abstract Task<bool> ProcessAsync(ProcessContext context);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this._disposed = true;
        }
    }
}