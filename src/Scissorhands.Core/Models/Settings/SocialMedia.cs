namespace Scissorhands.Models.Settings
{
    /// <summary>
    /// This represents the social media URL.
    /// </summary>
    public class SocialMedia
    {
        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the Twitter URL. eg) https://twitter.com/getscissorhands
        /// </summary>
        public string Twitter { get; set; }

        /// <summary>
        /// Gets or sets the Facebook URL. eg) https://facebook.com/getscissorhands
        /// </summary>
        public string Facebook { get; set; }

        /// <summary>
        /// Gets or sets the Google+ URL. eg) https://plus.google.com/u/0/+getscissorhands
        /// </summary>
        public string GooglePlus { get; set; }

        /// <summary>
        /// Gets or sets the Instagram URL. eg) https://instagram.com/getscissorhands
        /// </summary>
        public string Instagram { get; set; }

        /// <summary>
        /// Gets or sets the LinkedIn URL. eg) https://linkedin.com/in/getscissorhands
        /// </summary>
        public string LinkedIn { get; set; }

        /// <summary>
        /// Gets or sets the GitHub URL. eg) https://github.com/GetScissorhands
        /// </summary>
        public string GitHub { get; set; }
    }
}