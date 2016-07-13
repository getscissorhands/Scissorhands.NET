namespace Scissorhands.Models.Settings
{
    /// <summary>
    /// This specifies the publish mode of the post/page.
    /// </summary>
    public enum PublishMode
    {
        /// <summary>
        /// Indicates that no publish mode has been identified.
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// Indicates the preview mode.
        /// </summary>
        Preview = 1,

        /// <summary>
        /// Indicates the parse mode.
        /// </summary>
        Parse = 2,

        /// <summary>
        /// Indicates the publish mode.
        /// </summary>
        Publish = 3
    }
}