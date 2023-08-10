# Reliable Service Communication Demo script

This demo capture changes shown in the Pull Request https://github.com/KSchlobohm/devup-dont-skip/pull/2/files

@TODO - update steps with changes to the `_backToBackExceptionCount` variable or modify demo to always expect the same error behavior.

## Pre-requisite
We're going to assume your code supports Dependency Injection. That's not a requirement but it makes it easier to demo.

## Step 1: Enable Server Errors
Simulate intermittent errors by creating reliable errors.

**Demo**

1. Find the `TestingConfig.cs` class and enable errors.

	> **Note**<br>
	> This Middleware class exists to demonstrate how our code can handle intermittent errors.

	```cs
	static bool error = true;
	```

1. Run the web app and observe

- The missing error handling results in runtime errors.

## Step 2: Adding Retry Pattern to the GetAll method
Fix the error by adding reliable service communication with Polly.

**Demo**

1. Add the *Polly* NuGet package

	> Polly version 7.0.0.0
	
1. View **Warnings** and use VisualStudio to fix bindingRedirects as recommended

    ...insert image...
	
1. Create a new class `ReliableRestaurantApiData`

	> Copy all of the code from the `RestaurantApiData.cs` class so we can override methods one by one
	
1. Observe the method `GetHttpClient()`

	> **Note**<br>
	> At least this client is static. Tons of errors in web apps trace back to managing HttpClient but I didn't want to make this a demo about using IHttpClientFactory but that would be a better idea than making your own HttpClient objects.

1. Modify the `GetAllAsync` method.

	```cs
	    public async Task<IEnumerable<Restaurant>> GetAllAsync()
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
	```

1. Add Policy variable and contstructor

    ```cs
    private static AsyncPolicy<HttpResponseMessage> _httpRetryPolicy;
    
    public ReliableRestaurantApiData(ILogger logger)
    {
        InitializePolicy(logger);
    }
    ```

1. Add a method `InitializePolicy`

        ```cs
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
                    });
            }
        }
        ```

1. Run the web app and observe

- The code works again. At least this endpoint does. We should fix the others!

## Step 3: Use Retry on all the methods
What happens if we Retry on every error?

**Demo**

1. Replace the `AddAsync` method

    ```cs
        public async Task<Restaurant> AddAsync(Restaurant restaurant)
        {
            if (restaurant == null)
            {
                throw new ArgumentNullException(nameof(restaurant));
            }

            string jsonRestaurant = JsonConvert.SerializeObject(restaurant);

            var response = await _httpRetryPolicy.ExecuteAsync(() =>
            {
                var content = new StringContent(jsonRestaurant, Encoding.UTF8, "application/json");
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
    ```
    
1. Run the web app and observe

- The code works. It added the same data multiple times. It wouldn't be great if that was a credit card charge API.

## Step 4: Add Circuit Breaker
Now the code is slower.

- What happens if the service starts to break all the time?

**Demo**

1. Modify Policy variable

    ```cs
    private static AsyncPolicyWrap<HttpResponseMessage> _httpRetryPolicy;
    ```

1. Add a method `InitializePolicy`

        ```cs
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
                            durationOfBreak: TimeSpan.FromSeconds(10) // Break for 30 seconds.
                        ));
            }
        }
        ```

1. Run the web app and observe

- And now the code is broke again. But this time we meant to do that!

## Step 5: Add Fallback logic
Now the code fails fast, but we're back to failing code.

- Let's use the Fallback approach and build graceful degradation into the app.

**Demo**

1. Modify the `GetAllAsync` method.
1. 
    ```cs
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
    ```
    
1. Run the web app and observe

- And now the web app displays the last known data for restaurants. We can support read operations, but probably need to show a try again later error for write or delete.