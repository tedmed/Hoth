using WeatherEye.Web.Handlers;

namespace WeatherEye.Web.Extensions
{
    internal static class HttpClientsBuilderExtension
    {
        internal static IServiceCollection AddHttpClients(this IServiceCollection services)
        {
            services.AddHttpClient<WeatherApiClient>(client =>
            {
                client.BaseAddress = new("https+http://apiservice");
            }).AddHttpMessageHandler<AuthorizationHandler>();
            

            services.AddHttpClient<CAPApiClient>(client =>
            {
                client.BaseAddress = new("https+http://apiservice");
            }).AddHttpMessageHandler<AuthorizationHandler>();


            services.AddHttpClient<UserApiClient>(client =>
            {
                client.BaseAddress = new("https+http://apiservice");
            }).AddHttpMessageHandler<AuthorizationHandler>();

            return services;
        }
    }
}
