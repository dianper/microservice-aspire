namespace Microservice.Aspire.Api.Services.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services
                .AddSingleton<AzureBlobStorageService>()
                .AddSingleton<AzureServiceBusService>()
                .AddSingleton<CsvReaderService>()
                .AddSingleton<MongoDbService>();

            return services;
        }

        public static IServiceCollection AddHostedServices(this IServiceCollection services)
        {
            services
                .AddHostedService<FileCollectorService>()
                .AddHostedService<FileDetailsService>();

            return services;
        }
    }
}
