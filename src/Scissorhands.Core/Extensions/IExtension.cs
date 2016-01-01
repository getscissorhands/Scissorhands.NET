using System;
using System.Threading.Tasks;

namespace Scissorhands.Extensions
{
    /// <summary>
    /// This provides interfaces to the classes that extend functionalities of Scissorhands.NET.
    /// </summary>
    public interface IExtension : IDisposable
    {
        /// <summary>
        /// Gets or sets the <see cref="ExtensionTypes"/> value.
        /// </summary>
        ExtensionTypes ExtensionTypes { get; set; }

        /// <summary>
        /// Executes the extension.
        /// </summary>
        /// <typeparam name="T">Type to return.</typeparam>
        /// <returns>Returns the instance with type T.</returns>
        Task<T> Execute<T>();
    }
}