namespace Aliencube.Scissorhands.Models
{
    /// <summary>
    /// This represents the entity for web app settings.
    /// </summary>
    public class WebAppSettings
    {
        /// <summary>
        /// Gets or sets the server.
        /// </summary>
        public virtual ServerType Server { get; set; }

        /// <summary>
        /// Gets or sets the theme.
        /// </summary>
        public virtual string Theme { get; set; }
    }
}
