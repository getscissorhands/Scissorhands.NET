namespace Aliencube.Scissorhands.Services.Extensions
{
    /// <summary>
    /// This represents the extension entity for <see cref="string"/>;
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Checks whether the given string starts with the character or not.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="check">Character value.</param>
        /// <returns>Returns <c>True</c>, if the given string starts with the checking character; otherwise returns <c>False</c>.</returns>
        public static bool StartsWith(this string value, char check)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var chars = value.ToCharArray();
            var result = chars[0].Equals(check);
            return result;
        }
    }
}