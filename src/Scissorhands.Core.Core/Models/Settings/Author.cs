namespace Scissorhands.Models.Settings
{
    /// <summary>
    /// This represents the entity of author.
    /// </summary>
    public class Author
    {
        /// <summary>
        /// Gets or sets the name of the author.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the bio of the author.
        /// </summary>
        public string Bio { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this author is default or not.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SocialMedia"/> object.
        /// </summary>
        public SocialMedia SocialMedia { get; set; }
    }
}