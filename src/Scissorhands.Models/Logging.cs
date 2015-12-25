namespace Aliencube.Scissorhands.Models
{
    /// <summary>
    /// This represents entity for logging.
    /// </summary>
    public class Logging
    {
        /// <summary>
        /// Gets or sets a value indicating whether include scopes.
        /// </summary>
        public bool IncludeScopes { get; set; }

        /// <summary>
        /// Gets or sets the log level.
        /// </summary>
        public LogLevel LogLevel { get; set; }
    }
}