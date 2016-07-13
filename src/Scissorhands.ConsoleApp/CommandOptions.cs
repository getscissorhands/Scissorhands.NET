namespace Scissorhands
{
    /// <summary>
    /// This represents the options entity for command-line arguments.
    /// </summary>
    public class CommandOptions
    {
        /// <summary>
        /// Gets or sets the source file in markdown or directory where markdown is located.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the theme name to be applied.
        /// </summary>
        public string Theme { get; set; }

        /// <summary>
        /// Gets or sets the output directory to store generated HTML files.
        /// </summary>
        public string OutputDirectory { get; set; }
    }
}