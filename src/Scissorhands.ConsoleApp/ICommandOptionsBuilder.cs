using System.Threading.Tasks;

namespace Scissorhands
{
    /// <summary>
    /// This provides interfaces to the <see cref="CommandOptionsBuilder"/> class.
    /// </summary>
    public interface ICommandOptionsBuilder
    {
        /// <summary>
        /// Builds options from command arguments.
        /// </summary>
        /// <param name="args">List of arguments.</param>
        /// <returns>Returns the <see cref="Options"/> built</returns>
        Task<Options> BuildAsync(string[] args);

        /// <summary>
        /// Gets the default options by reading <c>appsettings.json</c>.
        /// </summary>
        /// <param name="filepath">File path of the <c>appsettings.json</c>.</param>
        /// <returns>Returns the <see cref="Options"/> deserialised.</returns>
        Task<Options> GetDefaultOptionsAsyc(string filepath);
    }
}