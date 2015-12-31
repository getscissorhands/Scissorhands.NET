using Microsoft.AspNet.Mvc;

namespace Scissorhands.WebApp.Controllers
{
    /// <summary>
    /// This represents the controller entity for home.
    /// </summary>
    [Route("admin")]
    public class AdminController : Controller
    {
        /// <summary>
        /// Processes /home/index.
        /// </summary>
        /// <returns>Returns the view model.</returns>
        public IActionResult Index()
        {
            return this.View();
        }

        /// <summary>
        /// Processes /home/about.
        /// </summary>
        /// <returns>Returns the view model.</returns>
        [Route("about")]
        public IActionResult About()
        {
            this.ViewData["Message"] = "Your application description page.";

            return this.View();
        }

        /// <summary>
        /// Processes /home/contact.
        /// </summary>
        /// <returns>Returns the view model.</returns>
        [Route("contact")]
        public IActionResult Contact()
        {
            this.ViewData["Message"] = "Your contact page.";

            return this.View();
        }

        /// <summary>
        /// Processes /home/error.
        /// </summary>
        /// <returns>Returns the view model.</returns>
        [Route("error")]
        public IActionResult Error()
        {
            return this.View();
        }
    }
}