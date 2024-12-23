namespace Microservice.Aspire.Api.Services
{
    public static class AzureServiceBusServiceExtensions
    {
        public static IServiceCollection AddAzureServiceBusService(this IServiceCollection services)
        {
            services.AddSingleton<AzureServiceBusService>();
            return services;
        }
    }
}
