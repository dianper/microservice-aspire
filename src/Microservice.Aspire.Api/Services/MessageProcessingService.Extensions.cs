namespace Microservice.Aspire.Api.Services
{
    public static class MessageProcessingServiceExtensions
    {
        public static IServiceCollection AddBackgroundService(this IServiceCollection services)
        {
            services.AddHostedService<MessageProcessingService>();
            return services;
        }
    }
}
