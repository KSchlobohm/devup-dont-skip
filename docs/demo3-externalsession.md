# External session script

This demo capture changes shown in the Pull Request https://github.com/KSchlobohm/devup-dont-skip/pull/6/files

## Pre-requisite
We're going to assume your code supports Dependency Injection. That's not a requirement but it makes it easier to demo.
The script assumes that we already have a Redis deployment and can connect.


## Step 1: Install the package
We modify behavior of the Web API so that nonce feature scales horizontally.

**Demo**

1. Add the  NuGet package

	> Install-Package Microsoft.Web.RedisSessionStateProvider -Version 5.0.0 -ProjectName OdeToFood.WebApi

1. Confirm the web.config has the new session provider added

1. Update the web.config

    ```xml
    <system.web>
        <sessionState mode="Custom" customProvider="MySessionStateStore">
			<providers>
				<add name="MySessionStateStore" type="Microsoft.Web.Redis.RedisSessionStateProvider" connectionString="MyRedisConnString"/>
			</providers>
		</sessionState>
    </system.web>
    ```

1. Add the connection string
    ```xml
		<add key="MyRedisConnString" value="[from azure portal... or external configuration]"/>
    ```

1. Get the connection string from Azure Portal

1. Show that the cache is empty

    ```sh
    keys *
    ```

1. Use configuration builder for "usersecrets"

    ```json
    {
        "MyRedisConnString":"from azure portal"
    }
    ```

## Step 2: Replace existing Caching

1. Add the Redis Package

    ```sh
    Install-Package StackExchange.Redis -Version 2.6.96 -ProjectName OdeToFood.WebApi
    ```

1. Replace System.Web.Caching

    ```cs
    private static readonly Lazy<ConnectionMultiplexer> _redisConnection = new Lazy<ConnectionMultiplexer>(() =>
    {
        return ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["MyRedisConnString"]);
    });

    private const string NonceCacheKeyPrefix = "RestaurantNonce:";
    ```

1. Update the PostRestaurant method

    ```cs
    // POST: api/Restaurants
    [ResponseType(typeof(Restaurant))]
    public async Task<IHttpActionResult> PostRestaurant(Restaurant restaurant)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        string nonce = Request.Headers.GetValues("X-Nonce")?.FirstOrDefault(); // Get the nonce value from the request headers

        IDatabase redisCache = _redisConnection.Value.GetDatabase();

        // Check if the nonce is already in the cache
        if (redisCache.KeyExists(NonceCacheKeyPrefix + nonce))
        {
            int existingId = Convert.ToInt32(redisCache.StringGet(NonceCacheKeyPrefix + nonce));
            // Duplicate request detected
            var existingRestaurant = await db.GetAsync(existingId);
            return CreatedAtRoute("DefaultApi", new { id = existingRestaurant.Id }, existingRestaurant);
        }

        var dto = await db.AddAsync(restaurant);

        // Store nonce in Redis
        TimeSpan cacheExpiration = TimeSpan.FromMinutes(5); // Set the desired expiration time
        redisCache.StringSet(NonceCacheKeyPrefix + nonce, dto.Id, cacheExpiration);

        return CreatedAtRoute("DefaultApi", new { id = dto.Id }, dto);
    }
    ```

## Step 3: Use Azure Portal to demonstrate

1. Modify the HomeController

    ```cs
        static int requestCount = 1;
        public ActionResult Index()
        {
            Session[Guid.NewGuid().ToString()] = $"Hello world{requestCount++}";
            return View();
        }
    ```

1. Show value for Key

    ```sh
    GET RestaurantNonce:d0ed99f8-1017-4ea0-8205-fae4c3ba5d97
    ```

1. Use the azure portal to view Redis console

    ```sh
    monitor
    ```

## Step 4: Now we need to move that Secret

1. Right+click then choose "manage user secrets"

* This installs config builders

1. In the open file we need to add our connection string

``` xml
    <?xml version="1.0" encoding="utf-8"?>
    <root>
        <secrets ver="1.0" >
            <secret name="MyRedisConnString" value="[from web.config]" />
        </secrets>
    </root>
```

