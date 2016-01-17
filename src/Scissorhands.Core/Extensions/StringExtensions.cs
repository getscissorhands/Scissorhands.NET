namespace Scissorhands.Extensions
{
    /// <summary>
    /// This represents the extention entity for string.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Gets the root path if the given base path is null or empty.
        /// </summary>
        /// <param name="value">Babe path value.</param>
        /// <returns>Returns the base path or root path.</returns>
        public static string OrRootPath(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "/";
            }

            if (!value.StartsWith("/"))
            {
                return $"/{value}";
            }

            return value;
        }

        /// <summary>
        /// Removes the trailing slash from the given URL.
        /// </summary>
        /// <param name="url">URL value.</param>
        /// <returns>Returns the URL without trailing slash.</returns>
        public static string TrimTrailingSlash(this string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return string.Empty;
            }

            var trimmed = url.TrimEnd('/');
            return trimmed;
        }
    }
}