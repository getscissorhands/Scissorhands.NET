namespace Aliencube.Scissorhands.Services.Configs
{
    /// <summary>
    /// This represents the entity for directories settings.
    /// </summary>
    public class Directories
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Directories"/> class.
        /// </summary>
        public Directories()
        {
            this.Themes = "themes";
            this.Posts = "posts";
            this.Published = "published";
        }

        /// <summary>
        /// Gets or sets the directory for themes. Default value is <c>themes</c>.
        /// </summary>
        public string Themes { get; set; }

        /// <summary>
        /// Gets or sets the directory for posts. Default value is <c>posts</c>.
        /// </summary>
        public string Posts { get; set; }

        /// <summary>
        /// Gets or sets the directory for published posts. Default value is <c>published</c>.
        /// </summary>
        public string Published { get; set; }
    }
}