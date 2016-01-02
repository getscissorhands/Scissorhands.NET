namespace Scissorhands.Models.Settings
{
    /// <summary>
    /// This specifies the type of feed.
    /// </summary>
    public enum FeedType
    {
        /// <summary>
        /// Indicates that no feed type is defined.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates that RSS is the feed type.
        /// </summary>
        Rss = 1,

        /// <summary>
        /// Indicates that ATOM is the feed type.
        /// </summary>
        Atom = 2,
    }
}