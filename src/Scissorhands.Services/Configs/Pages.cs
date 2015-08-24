namespace Aliencube.Scissorhands.Services.Configs
{
    /// <summary>
    /// This represents the entity for page settings.
    /// </summary>
    public class Pages
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Pages" /> class.
        /// </summary>
        public Pages()
        {
            this.Items = 10;
            this.Format = "page-{index}";
        }

        /// <summary>
        /// Gets or sets the number of posts in one page. Default value is <c>10</c>.
        /// </summary>
        public int Items { get; set; }

        /// <summary>
        /// Gets or sets the format of the filename format. Default value is <c>page-{index}</c>.
        /// </summary>
        public string Format { get; set; }
    }
}