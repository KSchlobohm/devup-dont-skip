
## Pre-requisite
We're going to assume your code supports Dependency Injection. That's not a requirement but it makes it easier to demo.
The script assumes that we already have a SQL database in our subscription.


1. Add EntityFramework package

    ```sh
    Install-Package EntityFramework -Version 6.4.4 -ProjectName OdeToFood.WebApi
    ```

    ```sh
    Install-Package Azure.Identity -Version 1.10.0 -ProjectName OdeToFood.WebApi
    ```

1. Add new class for DbContext (snippet)

```
SqlRestaurantContext.cs
```

1. Add new class for Repository implementation (snippet)

```
SqlRestaurantData.cs
```

1. Add new class for AzureSqlRetry (snippet)

```
MyConfiguration.cs
```

1. Change dependency injection

    ```cs
    internal static void RegisterContainer(HttpConfiguration httpConfiguration)
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(typeof(WebApiApplication).Assembly);
            builder.RegisterApiControllers(typeof(WebApiApplication).Assembly);
            builder.RegisterType<SqlRestaurantContext>()
                    .InstancePerRequest();
            builder.RegisterType<SqlRestaurantData>()
                    .As<IRestaurantData>()
                    .InstancePerRequest();

            var container = builder.Build();
    ```

1. Add connection string

```xml
	<connectionStrings>
		<add name="DefaultConnection"
			 providerName="System.Data.SqlClient"
			 connectionString="server=tcp:devup-dontskip-odetofood.database.windows.net;database=devup-dontskip-odetofood;"/>
	</connectionStrings>
```

1. Observe - connection using DefaultAzureCredential
1. Observe - Visual Studio Configuration

    ```sh
    Azure Service Authentication
    ```