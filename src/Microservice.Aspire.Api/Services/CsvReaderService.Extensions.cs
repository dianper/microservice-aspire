namespace Microservice.Aspire.Api.Services
{
    public static class CsvReaderServiceExtensions
    {
        public static IServiceCollection AddCsvReaderService(this IServiceCollection services)
        {
            services.AddSingleton<CsvReaderService>();
            return services;
        }
    }
}
