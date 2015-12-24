namespace Aliencube.Scissorhands.Services.Helpers
{
    /// <summary>
    /// This represents the helper entity for files.
    /// </summary>
    public class FileHelper : IFileHelper
    {
        private bool _disposed;

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>Returns the name.</returns>
        public string GetName(string name)
        {
            return name;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this._disposed = true;
        }
    }
}