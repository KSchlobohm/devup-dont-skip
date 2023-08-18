using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web;

namespace OdeToFood.WebApi.Models
{
    public class MyConfiguration : DbConfiguration
    {
        public MyConfiguration()
        {
            // https://learn.microsoft.com/en-us/ef/ef6/fundamentals/connection-resiliency/retry-logic#enabling-an-execution-strategy
            SetExecutionStrategy("System.Data.SqlClient", () => new SqlAzureExecutionStrategy());
        }
    }
}