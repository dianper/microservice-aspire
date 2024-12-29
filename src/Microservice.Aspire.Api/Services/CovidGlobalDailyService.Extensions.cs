namespace Microservice.Aspire.Api.Services
{
    public static class CovidGlobalDailyServiceExtensions
    {
        public static IServiceCollection AddCovidDailyBackgroundService(this IServiceCollection services)
        {
            services.AddHostedService<CovidGlobalDailyService>();
            return services;
        }
    }
}
