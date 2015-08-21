using System;

namespace Aliencube.Scissorhands.Services.Models
{
    /// <summary>
    /// This represents the model entity. This must be inherited.
    /// </summary>
    public abstract class BaseModel
    {
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the author.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets the date released.
        /// </summary>
        public DateTime DateReleased { get; set; }
    }
}