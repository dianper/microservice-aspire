namespace Microservice.Aspire.Api.Services
{
    public static class AzureBlobStorageServiceExtensions
    {
        public static IServiceCollection AddAzureBlobStorageService(this IServiceCollection services)
        {
            services.AddSingleton<AzureBlobStorageService>();
            return services;
        }
    }
}
