namespace Aliencube.Scissorhands.Services.Configs
{
    /// <summary>
    /// This represents the entity for content settings.
    /// </summary>
    public class Contents
    {
        private string _extension;

        /// <summary>
        /// Initialises a new instance of the <see cref="Contents"/> class.
        /// </summary>
        public Contents()
        {
            this.Theme = "default";
            this._extension = ".md";
            this.Pages = new Pages();
            this.Archives = "archives";
            this.Tags = "tags";
        }

        /// <summary>
        /// Gets or sets the name of the theme. Default theme is <c>default</c>.
        /// </summary>
        public string Theme { get; set; }

        /// <summary>
        /// Gets or sets the markdown file extension. Default value is <c>.md</c>.
        /// </summary>
        public string Extension
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(this._extension) && !this._extension.StartsWith("."))
                {
                    return "." + this._extension;
                }

                return this._extension;
            }

            set
            {
                this._extension = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Pages" /> object.
        /// </summary>
        public Pages Pages { get; set; }

        /// <summary>
        /// Gets or sets the directory name for archives. Default value is <c>archives</c>.
        /// </summary>
        public string Archives { get; set; }

        /// <summary>
        /// Gets or sets the directory name for tags. Default value is <c>tags</c>.
        /// </summary>
        public string Tags { get; set; }
    }
}