using System;
using System.Collections.Generic;

namespace Aliencube.Scissorhands.Services.Models
{
    /// <summary>
    /// This represents the model entity. This must be inherited.
    /// </summary>
    public abstract class BasePageModel
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="BasePageModel" /> class.
        /// </summary>
        protected BasePageModel()
        {
            this.Extensions = new Dictionary<Type, BaseExtensionModel>();
        }

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

        /// <summary>
        /// Gets or sets the post.
        /// </summary>
        public string Post { get; set; }

        /// <summary>
        /// Gets the list of <see cref="BaseExtensionModel" /> instances.
        /// </summary>
        public Dictionary<Type, BaseExtensionModel> Extensions { get; private set; }
    }
}