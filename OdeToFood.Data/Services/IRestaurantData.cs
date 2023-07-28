using OdeToFood.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OdeToFood.Data.Services
{
    public interface IRestaurantData
    {
        Task<IEnumerable<Restaurant>> GetAllAsync();
        Task<Restaurant> GetAsync(int id);
        Task<Restaurant> AddAsync(Restaurant restaurant);
        Task UpdateAsync(Restaurant restaurant);
        Task DeleteAsync(int id);
    }
}
