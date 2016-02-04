using System;
using System.IO;
using System.Linq;

using Microsoft.AspNet.Mvc;

using Scissorhands.Helpers;
using Scissorhands.Models.Settings;

namespace Scissorhands.WebApp.Controllers
{
    /// <summary>
    /// This represents the controller entity for home.
    /// </summary>
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly WebAppSettings _settings;
        private readonly IFileHelper _fileHelper;

        public AdminController(WebAppSettings settings, IFileHelper fileHelper)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            this._settings = settings;

            if (fileHelper == null)
            {
                throw new ArgumentNullException(nameof(fileHelper));
            }

            this._fileHelper = fileHelper;
        }

        /// <summary>
        /// Processes /home/index.
        /// </summary>
        /// <returns>Returns the view model.</returns>
        public IActionResult Index()
        {
            var dir = this._fileHelper.GetDirectory(this._settings.HtmlPath);
            var files = Directory.GetFiles(dir, "*.html", SearchOption.AllDirectories)
                                 .OrderByDescending(p => p)
                                 .Select(p => new FileInfo(p));

            this.ViewBag.Files = files;
            return this.View();
        }
    }
}