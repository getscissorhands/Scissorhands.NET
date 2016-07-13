using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Scissorhands
{
    /// <summary>
    /// This represents the entity for the <c>appsettings.json</c>.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Gets or sets the options from command-lines.
        /// </summary>
        public CommandOptions CommandOptions { get; set; }
    }
}
