using System.Collections.Generic;

namespace Scissorhands.Extensions
{
    /// <summary>
    /// This represents the extension entity for <see cref="List{T}"/>.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Gets the default instance of the <see cref="List{T}"/>, if the given value is null.
        /// </summary>
        /// <param name="value"><see cref="List{T}"/> object.</param>
        /// <typeparam name="T">Type of instance.</typeparam>
        /// <returns>Returns the default instance of the <see cref="List{T}"/>, if the given value is null.</returns>
        public static List<T> OrDefault<T>(this List<T> value)
        {
            return value ?? new List<T>();
        }
    }
}