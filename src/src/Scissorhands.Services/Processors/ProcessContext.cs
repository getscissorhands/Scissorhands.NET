using System.Collections.Generic;

namespace Aliencube.Scissorhands.Services.Processors
{
    /// <summary>
    /// This represents the context entity for the <see cref="BaseProcessor" /> class.
    /// </summary>
    public class ProcessContext
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="ProcessContext" /> class.
        /// </summary>
        /// <param name="items">
        /// The list of items for context.
        /// </param>
        public ProcessContext(IDictionary<string, object> items = null)
        {
            if (items == null)
            {
                this.Items = new Dictionary<string, object>();
                return;
            }

            this.Items = items;
        }

        /// <summary>
        /// Gets the list of items for context.
        /// </summary>
        public IDictionary<string, object> Items { get; private set; }
    }
}