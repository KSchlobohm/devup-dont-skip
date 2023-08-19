using OdeToFood.Data.Models;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

public class SqlRestaurantContext : DbContext
{
    public DbSet<Restaurant> Restaurants { get; set; }

    private static readonly System.Web.Caching.Cache _cache = new System.Web.Caching.Cache();

    public SqlRestaurantContext() : base("name=DefaultConnection")
    {
        // Use DefaultAzureCredential to authenticate to Azure SQL Database
        // https://learn.microsoft.com/en-us/azure/app-service/tutorial-connect-msi-sql-database?tabs=windowsclient%2Cef%2Cdotnet#3-modify-your-project
        var conn = (System.Data.SqlClient.SqlConnection)Database.Connection;

        var credential = new Azure.Identity.DefaultAzureCredential();

#if DEBUG
        string accessToken;
        if (_cache["db_token"] as string is null)
        {
            var token = credential.GetToken(new Azure.Core.TokenRequestContext(new[] { "https://database.windows.net/.default" }));
            accessToken = token.Token;
            // Add the nonce to the cache with a short expiration time
            _cache.Insert("db_token", accessToken, null, DateTime.UtcNow.AddMinutes(5), System.Web.Caching.Cache.NoSlidingExpiration);
        }
        else
        {
            accessToken = _cache["db_token"] as string;
        }
        conn.AccessToken = accessToken;
#else
        var token = credential.GetToken(new Azure.Core.TokenRequestContext(new[] { "https://database.windows.net/.default" }));
        conn.AccessToken = token.Token;
#endif

        // if using Microsoft.Data.SqlClient (default with EFCore)
        // then you can skip the above code and accomplish the same thing with just a connection string change
        // https://learn.microsoft.com/en-us/sql/connect/ado-net/introduction-microsoft-data-sqlclient-namespace?view=sql-server-ver16#azure-active-directory-managed-identity-authentication
    }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Restaurant>()
            .HasKey(t => t.Id)
            .Property(t => t.Id)
            .IsRequired()
            .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

        modelBuilder.Entity<Restaurant>()
            .Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);
    }
}
