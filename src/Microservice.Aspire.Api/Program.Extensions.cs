﻿using Asp.Versioning;
using Microservice.Aspire.Api.Services;

namespace Microservice.Aspire.Api
{
    public static class ProgramExtensions
    {
        public static IHostApplicationBuilder AddApplicationDependencies(this IHostApplicationBuilder builder, IConfiguration configuration)
        {
            // Add the default services
            builder.AddServiceDefaults();

            // Azure Blob Storage
            builder.AddAzureBlobClient("blobs");

            // Azure ServiceBus
            builder.AddAzureServiceBusClient("servicebus", settings =>
            {
                settings.ConnectionString = configuration.GetConnectionString("serviceBus");
            });

            // MongoDB
            builder.AddMongoDBClient("filedb", settings =>
            {
                settings.ConnectionString = configuration.GetConnectionString("mongo");
            });

            // Redis Cache
            builder.AddRedisClient("cache");

            return builder;
        }

        public static IHostApplicationBuilder AddServiceDependencies(this IHostApplicationBuilder builder)
        {
            // Services
            builder.Services
                .AddAzureBlobStorageService()
                .AddAzureServiceBusService();

            // Problem Details
            builder.Services.AddProblemDetails();

            // Versioning
            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("X-Api-Version"));
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add Background Services
            builder.Services.AddBackgroundService();

            return builder;
        }
    }
}
