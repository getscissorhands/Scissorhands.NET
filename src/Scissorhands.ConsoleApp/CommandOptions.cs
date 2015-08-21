using CommandLine;
using CommandLine.Text;

namespace Scissorhands.ConsoleApp
{
    /// <summary>
    /// This represents the entity for commandline options.
    /// </summary>
    public class CommandOptions
    {
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
    }
}