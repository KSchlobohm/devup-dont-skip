using Newtonsoft.Json;
using OdeToFood.Data.Models;
using OdeToFood.Data.Services;
using Polly;
using Polly.CircuitBreaker;
using Polly.Wrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OdeToFood.WebUI.Services
{
    public class ReliableRestaurantApiData : IRestaurantData
    {
        private static AsyncPolicyWrap<HttpResponseMessage> _httpRetryPolicy;

        public ReliableRestaurantApiData(ILogger logger)
        {
            InitializePolicy(logger);
        }

        private void InitializePolicy(ILogger logger)
        {
            if (_httpRetryPolicy == null)
            {
                _httpRetryPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .WaitAndRetryAsync(new[]
                    {
                        TimeSpan.FromMilliseconds(100),
                        TimeSpan.FromMilliseconds(250),
                        TimeSpan.FromMilliseconds(350)
                    }, (exception, timeSpan) =>
                    {
                        // Add logic to be executed before each retry, such as logging
                        logger.LogError("This error will be retried", exception.Exception);
                    })
             .WrapAsync(Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                 .AdvancedCircuitBreakerAsync(
                     0.5, // Break on >=50% actions result in handled exceptions...
                     samplingDuration: TimeSpan.FromSeconds(10), // ... over any 10 second period
                     minimumThroughput: 8, // ... provided at least 8 actions in the 10 second period.
                     durationOfBreak: TimeSpan.FromSeconds(10) // Break for 10 seconds.
                 ));
            }
        }

        private static HttpClient _client;

        private HttpClient GetHttpClient()
        {
            if (_client == null)
            {
                _client = new HttpClient();
                // don't hard code... read configuration for API URL
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

            var nonceData = Guid.NewGuid().ToString();
            var response = await _httpRetryPolicy.ExecuteAsync(() =>
            {
                var content = new StringContent(jsonRestaurant, Encoding.UTF8, "application/json");
                content.Headers.Add("X-Nonce", nonceData);
                return GetHttpClient().PostAsync($"api/restaurants", content);
            });

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

        private static IEnumerable<Restaurant> _knownRestaurants;

        public async Task<IEnumerable<Restaurant>> GetAllAsync()
        {
            try
            {
                var response = await _httpRetryPolicy.ExecuteAsync(() => GetHttpClient().GetAsync($"api/restaurants"));

                if (response.IsSuccessStatusCode)
                {
                    using (HttpContent content = response.Content)
                    {
                        var data = await content.ReadAsStringAsync();
                        var restaurants = JsonConvert.DeserializeObject<IEnumerable<Restaurant>>(data);
                        if (_knownRestaurants == null)
                        {
                            _knownRestaurants = restaurants;
                        }
                        return restaurants;
                    }
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
            catch (BrokenCircuitException)
            {
                return _knownRestaurants;
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