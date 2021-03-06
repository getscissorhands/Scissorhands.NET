﻿using System;
using System.Threading.Tasks;

namespace Scissorhands.Extensions
{
    /// <summary>
    /// This provides interfaces to the classes that extend functionalities of Scissorhands.NET.
    /// </summary>
    public interface IExtension : IDisposable
    {
        /// <summary>
        /// Gets or sets the <see cref="Extensions.ExtensionTypes"/> value.
        /// </summary>
        ExtensionTypes ExtensionTypes { get; set; }

        /// <summary>
        /// Executes the extension.
        /// </summary>
        /// <returns>Returns the value as an error code.</returns>
        /// <remarks>Error code of 0 usually means success.</remarks>
        Task<int> ExecuteAsync();
    }
}