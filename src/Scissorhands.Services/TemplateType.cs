namespace Aliencube.Scissorhands.Services
{
    /// <summary>
    /// This specifies the template type.
    /// </summary>
    public enum TemplateType
    {
        /// <summary>
        /// Indicates that no template type is specified.
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// Specifies layout.
        /// </summary>
        Layout = 1,

        /// <summary>
        /// Specifies page.
        /// </summary>
        Page = 2,

        /// <summary>
        /// Specifies post.
        /// </summary>
        Post = 3,

        /// <summary>
        /// Specifies tag.
        /// </summary>
        Tag = 4,
    }
}