using RazorEngine.Text;

namespace Aliencube.Scissorhands.Services.Helpers
{
    /// <summary>
    /// This represents the helper entity for HTML rendering.
    /// </summary>
    public class HtmlHelper
    {
        /// <summary>
        /// Gets the raw HTML string.
        /// </summary>
        /// <param name="value">
        /// Encoded HTML string.
        /// </param>
        /// <returns>
        /// Returns HTML decoded string.
        /// </returns>
        public IEncodedString Raw(string value)
        {
            var converted = new RawString(value);
            return converted;
        }
    }
}