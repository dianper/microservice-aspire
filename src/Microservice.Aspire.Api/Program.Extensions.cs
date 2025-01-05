namespace Microservice.Aspire.Api;

using Asp.Versioning;
using Microservice.Aspire.Api.Configurations;
using Microservice.Aspire.Api.Services;
using Microservice.Aspire.Api.Services.Extensions;

public static class ProgramExtensions
{
    public static IHostApplicationBuilder AddApplicationDependencies(this IHostApplicationBuilder builder, IConfiguration configuration)
    {
        // Add the default services
        builder.AddServiceDefaults();

        // Azure Blob Storage
        builder.AddAzureBlobClient("blobs");

        // Azure ServiceBus
        builder.AddAzureServiceBusClient("servicebus",
            settings => settings.ConnectionString = configuration.GetConnectionString("serviceBus"));

        // MongoDB
        builder.AddMongoDBClient("mongodb",
            settings => settings.ConnectionString = configuration.GetConnectionString("mongo"));

        // Postgres
        builder.AddNpgsqlDbContext<ApiDbContext>("postgresdb",
            settings => settings.ConnectionString = configuration.GetConnectionString("postgres"));

        // Redis Cache
        builder.AddRedisClient("cache");

        return builder;
    }

    public static IHostApplicationBuilder AddServiceDependencies(this IHostApplicationBuilder builder, IConfiguration configuration)
    {
        // Settings
        builder.Services
            .Configure<FileCollectorSettings>(configuration.GetSection("FileCollector"))
            .Configure<MongoDbSettings>(configuration.GetSection("MongoDb"))
            .Configure<ServiceBusSettings>(configuration.GetSection("ServiceBus"));

        // HttpClient
        builder.Services.AddHttpClient<FileUploaderClient>(
            client => client.BaseAddress = new Uri("http://localhost:5014"));

        // Services
        builder.Services.AddServices();

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

        builder.Services.AddCors(options =>
        {
            // Test purposes only
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        builder.Services.AddControllers();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Add Background Services
        builder.Services.AddHostedServices();

        return builder;
    }

    public static IApplicationBuilder EnsureDatabaseCreated(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
        dbContext.Database.EnsureCreated();
        return app;
    }
}
