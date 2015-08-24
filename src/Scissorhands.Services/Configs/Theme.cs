namespace Aliencube.Scissorhands.Services.Configs
{
    /// <summary>
    /// This represents the entity for theme settings.
    /// </summary>
    public class Theme
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Theme" /> class.
        /// </summary>
        public Theme()
        {
            this.Name = "default";
            this.Master = "master.cshtml";
            this.Css = "css";
            this.Js = "js";
            this.Images = "images";
            this.Includes = "includes";
        }

        /// <summary>
        /// Gets or sets the theme name. Default value is <c>default</c>.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the filename of the master page. Default value is <c>master.cshtml</c>.
        /// </summary>
        /// <remarks>The file extension, <c>.cshtml</c> can be omitted. But if added, it must be <c>.cshtml</c>.</remarks>
        public string Master { get; set; }

        /// <summary>
        /// Gets or sets the directory name for CSS. Default value is <c>css</c>.
        /// </summary>
        public string Css { get; set; }

        /// <summary>
        /// Gets or sets the directory name for JavaScript. Default value is <c>js</c>.
        /// </summary>
        public string Js { get; set; }

        /// <summary>
        /// Gets or sets the directory name for images. Default value is <c>images</c>.
        /// </summary>
        public string Images { get; set; }

        /// <summary>
        /// Gets or sets the directory name for included template files consumed within the master. Default value is <c>includes</c>.
        /// </summary>
        public string Includes { get; set; }
    }
}