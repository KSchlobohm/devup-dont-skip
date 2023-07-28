using OdeToFood.Data.Models;
using OdeToFood.Data.Services;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace OdeToFood.WebApi.Controllers
{
    public class RestaurantsController : ApiController
    {
        private readonly IRestaurantData db;

        public RestaurantsController(IRestaurantData db)
        {
            this.db = db;
        }

        [ResponseType(typeof(IEnumerable<Restaurant>))]
        public async Task<IHttpActionResult> GetRestaurants()
        {
            return Ok(await db.GetAllAsync());
        }

        [ResponseType(typeof(Restaurant))]
        public async Task<IHttpActionResult> GetRestaurant(int id)
        {
            return Ok(await db.GetAsync(id));
        }

        // PUT: api/Restaurant/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutRestaurant(int id, Restaurant restaurant)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != restaurant.Id)
            {
                return BadRequest();
            }

            var existingRestaurant = await db.GetAsync(id);
            if (existingRestaurant == null)
            {
                return NotFound();
            }
            existingRestaurant.Name = restaurant.Name;
            existingRestaurant.Cuisine= restaurant.Cuisine;
            await db.UpdateAsync(existingRestaurant);

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Restaurants
        [ResponseType(typeof(Restaurant))]
        public async Task<IHttpActionResult> PostRestaurant(Restaurant restaurant)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await db.AddAsync(restaurant);

            var dto = new Restaurant()
            {
                Id = restaurant.Id,
                Cuisine = restaurant.Cuisine,
            };

            return CreatedAtRoute("DefaultApi", new { id = restaurant.Id }, dto);
        }

        // PUT: api/Restaurant/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> DeleteRestaurant(int id)
        {
            var existingRestaurant = await db.GetAsync(id);
            if (existingRestaurant == null)
            {
                return NotFound();
            }
            await db.DeleteAsync(id);

            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
