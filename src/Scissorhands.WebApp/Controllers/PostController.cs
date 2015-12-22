using Microsoft.AspNet.Mvc;

namespace Aliencube.Scissorhands.WebApp.Controllers
{
    /// <summary>
    /// This represents the controller entity for post.
    /// </summary>
    [Route("post")]
    public class PostController : Controller
    {
        /// <summary>
        /// Processes /post/index.
        /// </summary>
        /// <returns>Returns the view model.</returns>
        public IActionResult Index()
        {
            return this.RedirectToRoute("write");
        }

        /// <summary>
        /// Processes /post/write.
        /// </summary>
        /// <returns>Returns the view model.</returns>
        [Route("write", Name = "write")]
        public IActionResult Write()
        {
            return this.View();
        }

        /// <summary>
        /// Processes /post/edit.
        /// </summary>
        /// <returns>Returns the view model.</returns>
        [Route("edit")]
        public IActionResult Edit()
        {
            return this.View();
        }

        /// <summary>
        /// Processes /post/preview.
        /// </summary>
        /// <returns>Returns the view model.</returns>
        [Route("preview")]
        public IActionResult Preview()
        {
            return this.View();
        }

        /// <summary>
        /// Processes /post/save.
        /// </summary>
        /// <returns>Returns the view model.</returns>
        [Route("save")]
        public IActionResult Save()
        {
            return this.View();
        }

        /// <summary>
        /// Processes /post/build.
        /// </summary>
        /// <returns>Returns the view model.</returns>
        [Route("build")]
        public IActionResult Build()
        {
            return this.View();
        }

        /// <summary>
        /// Processes /post/view.
        /// </summary>
        /// <returns>Returns the view model.</returns>
        [Route("view")]
        public IActionResult PostView()
        {
            return this.View();
        }
    }
}