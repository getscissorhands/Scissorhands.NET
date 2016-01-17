using System;

namespace Scissorhands.Extensions
{
    /// <summary>
    /// This represents the extension entity for the <see cref="Uri"/> class.
    /// </summary>
    public static class UriExtensions
    {
        /// <summary>
        /// Removes the trailing slash of the URL.
        /// </summary>
        /// <param name="uri"><see cref="Uri"/> instance to extend.</param>
        /// <returns>Returns the URL without the trailing slash.</returns>
        public static string TrimTrailingSlash(this Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            var url = uri.ToString().TrimEnd('/');
            return url;
        }
    }
}