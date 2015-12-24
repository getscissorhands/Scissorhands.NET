using System;

namespace Aliencube.Scissorhands.Services.Helpers
{
    /// <summary>
    /// This provides interfaces to the <see cref="FileHelper"/> class.
    /// </summary>
    public interface IFileHelper : IDisposable
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>Returns the name.</returns>
        string GetName(string name);
    }
}