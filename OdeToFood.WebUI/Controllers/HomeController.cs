using OdeToFood.Data.Services;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace OdeToFood.WebUI.Controllers
{
    public class HomeController : Controller
    {
        IRestaurantData _api;

        public HomeController(IRestaurantData api)
        {
            this._api = api;
        }

        public async Task<ActionResult> Index()
        {
            var model = await _api.GetAllAsync();
            return View(model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}