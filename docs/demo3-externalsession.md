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

1. Use configuration builder for "usersecrets"

    ```json
    {
        "MyRedisConnString":"from azure portal"
    }
    ```

## Step 2: Use Azure Portal to demonstrate

1. Modify the HomeController

    ```cs
        static int requestCount = 1;
        public ActionResult Index()
        {
            Session[Guid.NewGuid().ToString()] = $"Hello world{requestCount++}";
            return View();
        }
    ```

1. Use the azure portal to view Redis console

    ```sh
    monitor
    ```