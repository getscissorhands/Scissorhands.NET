using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Aliencube.Scissorhands.WebApp.Controllers
{
    public class BuildController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return this.View();
        }
    }
}
