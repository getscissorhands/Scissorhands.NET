using System;

namespace Aliencube.Scissorhands.ConsoleApp.Interfaces
{
    /// <summary>
    /// This provides interfaces to the <see cref="CommandOptions" /> class.
    /// </summary>
    public interface ICommandOptions : IDisposable
    {
        /// <summary>
        /// Gets or sets the post filepath to publish.
        /// </summary>
        string Post { get; set; }

        /// <summary>
        /// Gets or sets the theme name to apply.
        /// </summary>
        string Theme { get; set; }

        /// <summary>
        /// Gets the usage from the helper text of each option.
        /// </summary>
        /// <returns>
        /// Returns the usage from the helper text of each option.
        /// </returns>
        string GetUsage();
    }
}