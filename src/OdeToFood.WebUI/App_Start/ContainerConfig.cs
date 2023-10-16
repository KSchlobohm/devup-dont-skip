using Autofac;
using Autofac.Integration.Mvc;
using OdeToFood.Data.Services;
using OdeToFood.WebUI.Services;
using System.Web.Mvc;

namespace OdeToFood.WebUI.App_Start
{
    public class ContainerConfig
    {
        internal static void RegisterContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterControllers(typeof(MvcApplication).Assembly);
            builder.RegisterType<ReliableRestaurantApiData>()
                   .As<IRestaurantData>()
                   .InstancePerRequest();
            builder.RegisterType<DiagnosticsLogger>()
                   .As<ILogger>()
                   .InstancePerDependency();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}