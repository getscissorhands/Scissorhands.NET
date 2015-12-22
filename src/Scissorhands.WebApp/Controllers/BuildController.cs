using Microsoft.AspNet.Mvc;

namespace Aliencube.Scissorhands.WebApp.Controllers
{
    /// <summary>
    /// This represents the controller entity for build.
    /// </summary>
    public class BuildController : Controller
    {
        /// <summary>
        /// Processes /build/index.
        /// </summary>
        /// <returns>Returns the view model.</returns>
        public IActionResult Index()
        {
            return this.View();
        }
    }
}