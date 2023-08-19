using OdeToFood.Data.Models;
using OdeToFood.Data.Services;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace OdeToFood.WebUI.Controllers
{
    public class RestaurantsController : Controller
    {
        private readonly IRestaurantData _api;

        public RestaurantsController(IRestaurantData api)
        {
            _api = api;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var model = await _api.GetAllAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            var model = await _api.GetAsync(id);
            if (model == null)
            {
                return View("NotFound");
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View(new Restaurant());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Restaurant restaurant)
        {
            if (ModelState.IsValid)
            {
                var newRestaurant = await _api.AddAsync(restaurant);
                return RedirectToAction("Details", new { id = newRestaurant.Id });
            }
            return View(new Restaurant());
        }
        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            var model = await _api.GetAsync(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Restaurant restaurant)
        {
            if (ModelState.IsValid)
            {
                await _api.UpdateAsync(restaurant);
                TempData["Message"] = "You have saved the restaurant!";
                return RedirectToAction("Details", new { id = restaurant.Id });
            }
            return View(restaurant);
        }

        [HttpGet]
        public async Task<ActionResult> Delete(int id)
        {
            var model = await _api.GetAsync(id);
            if (model == null)
            {
                return View("NotFound");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, FormCollection form)
        {
            await _api.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}