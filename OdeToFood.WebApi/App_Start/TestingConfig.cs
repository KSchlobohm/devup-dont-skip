using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace OdeToFood.WebApi.App_Start
{
    public class TestingConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Log Request
            // config.MessageHandlers.Add(new IntermittentErrorRequestHandler());

            // ... other configurations
        }
    }

    public class IntermittentErrorRequestHandler : DelegatingHandler
    {
        static int _requestCount = 0;
        static int _backToBackExceptionCount = 4;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_backToBackExceptionCount > 0 && ++_requestCount % (_backToBackExceptionCount+1) > 0)
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
                response.Content = new StringContent("Intermittent Error");
                return response;
            }

            return await base.SendAsync(request, cancellationToken); ;
        }
    }
}