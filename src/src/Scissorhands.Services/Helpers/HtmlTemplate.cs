using RazorEngine.Templating;

namespace Aliencube.Scissorhands.Services.Helpers
{
    /// <summary>
    /// This represents the template entity for HTML.
    /// </summary>
    /// <typeparam name="T">
    /// Model type used in the razor templates.
    /// </typeparam>
    public class HtmlTemplate<T> : TemplateBase<T> where T : class
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="HtmlTemplate{T}" /> class.
        /// </summary>
        public HtmlTemplate()
        {
            this.Html = new HtmlHelper();
        }

        /// <summary>
        /// Gets the <see cref="HtmlHelper" /> instance.
        /// </summary>
        public HtmlHelper Html { get; private set; }
    }
}