using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Aliencube.Scissorhands.Services.Helpers;

namespace Aliencube.Scissorhands.Services
{
    /// <summary>
    /// This represents the service entity for build.
    /// </summary>
    public class BuildService
    {
        private readonly IFileHelper _fileHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildService"/> class.
        /// </summary>
        /// <param name="fileHelper"><see cref="IFileHelper"/> instance.</param>
        public BuildService(IFileHelper fileHelper)
        {
            if (fileHelper == null)
            {
                throw new ArgumentNullException(nameof(fileHelper));
            }

            this._fileHelper = fileHelper;
        }

        /// <summary>
        /// The build.
        /// </summary>
        public void Build()
        {
        }

        public async Task<string> PublishMarkdown(string markdown)
        {
            throw new NotImplementedException();
        }

        public async Task<string> PublishPost(string markdown)
        {
            throw new NotImplementedException();
        }
    }
}
