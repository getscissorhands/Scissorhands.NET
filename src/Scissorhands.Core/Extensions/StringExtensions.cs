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
        public static string GetRootPathIfNullOrEmpty(this string value)
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
    }
}