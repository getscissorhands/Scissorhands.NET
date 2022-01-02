namespace Scissorhands.Core
{
    /// <summary>
    /// This represents the entity for the frontmatter of content.
    /// </summary>
    public class FrontMatter
    {
        /// <summary>
        /// Gets the title.
        /// </summary>
        public virtual string? Title { get; private set; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        public virtual string? Description { get; private set; }

        /// <summary>
        /// Gets the slug.
        /// </summary>
        public virtual string? Slug { get; private set; }

        /// <summary>
        /// Gets the order.
        /// </summary>
        public virtual int? Order { get; private set; }
    }
}