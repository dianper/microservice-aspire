namespace Microservice.Aspire.Api.Services
{
    public static class MongoDbServiceExtensions
    {
        public static IServiceCollection AddMongoDbService(this IServiceCollection services)
        {
            services.AddSingleton<MongoDbService>();
            return services;
        }
    }
}
