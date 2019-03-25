using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DemoWebsite.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var environment = Environment.GetEnvironmentVariables();
            var variables = environment.Keys.OfType<string>().ToDictionary(key => key, key => environment[key]?.ToString());

            return View(variables);
        }
    }
}