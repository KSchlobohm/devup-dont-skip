using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OdeToFood.WebApi.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        static int requestCount = 1;
        public ActionResult Index()
        {
            Session[Guid.NewGuid().ToString()] = $"Hello world{requestCount++}";
            return View();
        }
    }
}