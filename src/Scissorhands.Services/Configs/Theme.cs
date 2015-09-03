namespace Aliencube.Scissorhands.Services.Configs
{
    /// <summary>
    /// This represents the entity for theme settings.
    /// </summary>
    public class Theme
    {
        private string _master;
        private string _page;
        private string _post;
        private string _tag;

        /// <summary>
        /// Initialises a new instance of the <see cref="Theme" /> class.
        /// </summary>
        public Theme()
        {
            this.Name = "default";
            this._master = "master.cshtml";
            this._page = "page.cshtml";
            this._post = "post.cshtml";
            this._tag = "tag.cshtml";
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
        public string Master
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(this._master) && !this._master.ToLowerInvariant().EndsWith(".cshtml"))
                {
                    return this._master + ".cshtml";
                }

                return this._master;
            }

            set
            {
                this._master = value;
            }
        }

        /// <summary>
        /// Gets or sets the filename of the generic page. Default value is <c>page.cshtml</c>.
        /// </summary>
        /// <remarks>The file extension, <c>.cshtml</c> can be omitted. But if added, it must be <c>.cshtml</c>.</remarks>
        public string Page
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(this._page) && !this._page.ToLowerInvariant().EndsWith(".cshtml"))
                {
                    return this._page + ".cshtml";
                }

                return this._page;
            }

            set
            {
                this._page = value;
            }
        }

        /// <summary>
        /// Gets or sets the filename of the post page. Default value is <c>post.cshtml</c>.
        /// </summary>
        /// <remarks>The file extension, <c>.cshtml</c> can be omitted. But if added, it must be <c>.cshtml</c>.</remarks>
        public string Post
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(this._post) && !this._post.ToLowerInvariant().EndsWith(".cshtml"))
                {
                    return this._post + ".cshtml";
                }

                return this._post;
            }

            set
            {
                this._post = value;
            }
        }

        /// <summary>
        /// Gets or sets the filename of the tag page. Default value is <c>tag.cshtml</c>.
        /// </summary>
        /// <remarks>The file extension, <c>.cshtml</c> can be omitted. But if added, it must be <c>.cshtml</c>.</remarks>
        public string Tag
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(this._tag) && !this._tag.ToLowerInvariant().EndsWith(".cshtml"))
                {
                    return this._tag + ".cshtml";
                }

                return this._tag;
            }

            set
            {
                this._tag = value;
            }
        }

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