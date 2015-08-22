using Aliencube.Scissorhands.Services.Interfaces;

using CommandLine;
using CommandLine.Text;

namespace Aliencube.Scissorhands.Services
{
    /// <summary>
    /// This represents the entity for commandline options.
    /// </summary>
    public class CommandOptions : ICommandOptions
    {
        private bool _disposed;

        /// <summary>
        /// Gets or sets the post filepath to publish.
        /// </summary>
        [Option('p', "post",
            Required = false,
            HelpText = "Filename of the post to publish. If omitted, entire posts will be processed.",
            DefaultValue = null)]
        public string Post { get; set; }

        /// <summary>
        /// Gets or sets the theme name to apply.
        /// </summary>
        [Option('t', "theme",
            Required = false,
            HelpText = "Theme for post to apply",
            DefaultValue = "default")]
        public string Theme { get; set; }

        /// <summary>
        /// Gets the usage from the helper text of each option.
        /// </summary>
        /// <returns>
        /// Returns the usage from the helper text of each option.
        /// </returns>
        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
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