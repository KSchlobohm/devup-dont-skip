using Newtonsoft.Json;
using OdeToFood.Data.Models;
using OdeToFood.Data.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace OdeToFood.WebUI.Services
{
    public class RestaurantApiData : IRestaurantData
    {
        private static HttpClient _client;
        
        private HttpClient GetHttpClient()
        {
            if (_client == null)
            {
                _client = new HttpClient();
                // TODO - read configuration for API URL
                _client.BaseAddress = new Uri("https://localhost:44309");
            }
            return _client;
        }   

        public async Task<Restaurant> AddAsync(Restaurant restaurant)
        {
            if (restaurant == null)
            {
                throw new ArgumentNullException(nameof(restaurant));
            }

            string jsonRestaurant = JsonConvert.SerializeObject(restaurant);
            StringContent content = new StringContent(jsonRestaurant, Encoding.UTF8, "application/json");

            var response = await GetHttpClient().PostAsync($"api/restaurants", content);

            if (response.IsSuccessStatusCode)
            {
                using (HttpContent responseContent = response.Content)
                {
                    var data = await responseContent.ReadAsStringAsync();
                    var responseRestaurant = JsonConvert.DeserializeObject<Restaurant>(data);
                    return responseRestaurant;
                }
            }
            else
            {
                throw new Exception(response.ReasonPhrase);
            }
        }

        public async Task DeleteAsync(int id)
        {
            var response = await GetHttpClient().DeleteAsync($"api/restaurants/{id}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.ReasonPhrase);
            }
        }

        public async Task<Restaurant> GetAsync(int id)
        {
            var response = await GetHttpClient().GetAsync($"api/restaurants/{id}");

            if (response.IsSuccessStatusCode)
            {
                using (HttpContent content = response.Content)
                {
                    var data = await content.ReadAsStringAsync();
                    var restaurant = JsonConvert.DeserializeObject<Restaurant>(data);
                    return restaurant;
                }
            }
            else
            {
                throw new Exception(response.ReasonPhrase);
            }
        }

        public async Task<IEnumerable<Restaurant>> GetAllAsync()
        {
            var response = await GetHttpClient().GetAsync($"api/restaurants");

            if (response.IsSuccessStatusCode)
            {
                using (HttpContent content = response.Content)
                {
                    var data = await content.ReadAsStringAsync();
                    var restaurants = JsonConvert.DeserializeObject<IEnumerable<Restaurant>>(data);
                    return restaurants;
                }
            }
            else
            {
                throw new Exception(response.ReasonPhrase);
            }
        }

        public async Task UpdateAsync(Restaurant restaurant)
        {
            if (restaurant == null)
            {
                throw new ArgumentNullException(nameof(restaurant));
            }

            string jsonRestaurant = JsonConvert.SerializeObject(restaurant);
            StringContent content = new StringContent(jsonRestaurant, Encoding.UTF8, "application/json");

            var response = await GetHttpClient().PutAsync($"api/restaurants/{restaurant.Id}", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.ReasonPhrase);
            }
        }
    }
}