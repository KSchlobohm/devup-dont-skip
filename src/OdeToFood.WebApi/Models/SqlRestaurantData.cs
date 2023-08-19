using OdeToFood.Data.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace OdeToFood.Data.Services
{
    public class SqlRestaurantData : IRestaurantData
    {
        private SqlRestaurantContext _context;

        public SqlRestaurantData(SqlRestaurantContext context)
        {
            _context = context;
        }

        public async Task<Restaurant> AddAsync(Restaurant restaurant)
        {
            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();

            return restaurant;
        }

        public async Task UpdateAsync(Restaurant restaurant)
        {
            var existing = _context.Restaurants.FirstOrDefault(r => r.Id == restaurant.Id);

            if (existing != null)
            {
                existing.Name = restaurant.Name;
                existing.Cuisine = restaurant.Cuisine;
            }

            await _context.SaveChangesAsync();
        }

        public Task<Restaurant> GetAsync(int id)
        {
            return _context.Restaurants.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Restaurant>> GetAllAsync()
        {
            return (await _context.Restaurants.OrderBy(r => r.Name).ToListAsync()).AsEnumerable();
        }

        public async Task DeleteAsync(int id)
        {
            var existing = _context.Restaurants.FirstOrDefault(r => r.Id == id);
            if (existing != null)
            {
                _context.Restaurants.Remove(existing);
                await _context.SaveChangesAsync();
            }
        }
    }
}
