using OdeToFood.Data.Models;
using OdeToFood.Data.Services;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private static readonly System.Web.Caching.Cache _cache = new System.Web.Caching.Cache();

        // POST: api/Restaurants
        [ResponseType(typeof(Restaurant))]
        public async Task<IHttpActionResult> PostRestaurant(Restaurant restaurant)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string nonce = Request.Headers.GetValues("X-Nonce")?.FirstOrDefault(); // Get the nonce value from the request headers

            // Check if the nonce is already in the cache
            if (_cache[nonce] != null)
            {
                // Duplicate request detected
                var existingRestaurant = await db.GetAsync((int)_cache[nonce]);
                return CreatedAtRoute("DefaultApi", new { id = existingRestaurant.Id }, existingRestaurant);
            }

            var dto = await db.AddAsync(restaurant);
            // Add the nonce to the cache with a short expiration time
            _cache.Insert(nonce, dto.Id, null, DateTime.UtcNow.AddMinutes(5), System.Web.Caching.Cache.NoSlidingExpiration);

            return CreatedAtRoute("DefaultApi", new { id = dto.Id }, dto);
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
