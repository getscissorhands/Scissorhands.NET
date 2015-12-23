using System;
using System.Threading.Tasks;

namespace Aliencube.Scissorhands.Services.Processors
{
    /// <summary>
    /// This provides interfaces to the classes inheriting the <see cref="BaseProcessor" /> class.
    /// </summary>
    public interface IProcessor : IDisposable
    {
        /// <summary>
        /// Processes the given context.
        /// </summary>
        /// <param name="context">
        /// The <see cref="ProcessContext" /> instance.
        /// </param>
        /// <returns>
        /// Returns <c>True</c>, if processed; otherwise returns <c>False</c>.
        /// </returns>
        bool Process(ProcessContext context);

        /// <summary>
        /// Processes the given context.
        /// </summary>
        /// <param name="context">
        /// The <see cref="ProcessContext" /> instance.
        /// </param>
        /// <returns>
        /// Returns <c>True</c>, if processed; otherwise returns <c>False</c>.
        /// </returns>
        Task<bool> ProcessAsync(ProcessContext context);
    }
}