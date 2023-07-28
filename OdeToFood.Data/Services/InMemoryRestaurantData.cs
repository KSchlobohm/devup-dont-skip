using OdeToFood.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OdeToFood.Data.Services
{
    public class InMemoryRestaurantData : IRestaurantData
    {
        List<Restaurant> restaurants;

        public InMemoryRestaurantData()
        {
            restaurants = new List<Restaurant>()
            {
                new Restaurant { Id = 1, Name = "Scott's Pizza", Cuisine = CuisineType.Italian},
                new Restaurant { Id = 2, Name = "Tersiguels", Cuisine = CuisineType.French},
                new Restaurant { Id = 3, Name = "Mango Grove", Cuisine = CuisineType.Indian},
            };
        }

        public Task<Restaurant> AddAsync(Restaurant restaurant)
        {
            restaurants.Add(restaurant);
            restaurant.Id = restaurants.Max(r => r.Id) + 1;
            return Task.FromResult(restaurant);
        }

        public async Task UpdateAsync(Restaurant restaurant)
        {
            var existing = await GetAsync(restaurant.Id);
            if (existing != null)
            {
                existing.Name = restaurant.Name;
                existing.Cuisine = restaurant.Cuisine;
            }
        }

        public Task<Restaurant> GetAsync(int id)
        {
            return Task.FromResult(restaurants.FirstOrDefault(r => r.Id == id));
        }

        public Task<IEnumerable<Restaurant>> GetAllAsync()
        {
            return Task.FromResult(restaurants.OrderBy(r => r.Name).AsEnumerable());
        }

        public async Task DeleteAsync(int id)
        {
            var restaurant = await GetAsync(id);
            if (restaurant != null)
            {
                restaurants.Remove(restaurant);
            }
        }
    }
}
