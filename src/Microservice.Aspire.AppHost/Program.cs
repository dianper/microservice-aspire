using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Sql Server
var sqlPass = builder.AddParameter("password", "Password123!");

var sql = builder
    .AddSqlServer("sql", port: 58349, password: sqlPass)
    .WithLifetime(ContainerLifetime.Persistent);

// Azure ServiceBus (Emulator)
var configFile = Path.Combine(Directory.GetCurrentDirectory(), "Config.json");

var messageBus = builder
    .AddContainer("servicebus", "mcr.microsoft.com/azure-messaging/servicebus-emulator")
    .WithReference(sql.Resource.PrimaryEndpoint)
    .WithEndpoint(port: 5672, targetPort: 5672, name: "servicebus")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("SQL_SERVER", "sql")
    .WithEnvironment("MSSQL_SA_PASSWORD", "Password123!")
    .WithBindMount(configFile, "/ServiceBus_Emulator/ConfigFiles/Config.json")
    .WaitFor(sql);

var messageBusEndpoint = messageBus.GetEndpoint("servicebus");

// Redis Cache
var redis = builder
    .AddRedis("cache")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithRedisInsight();

// Azure Storage (Emulator)
var storage = builder
    .AddAzureStorage("storage")
    .RunAsEmulator(azurite =>
    {
        azurite
            .WithBlobPort(27000)
            .WithQueuePort(27001)
            .WithTablePort(27002)
            .WithDataVolume();
    })
    .AddBlobs("blobs");

// MongoDB
var mongo = builder
    .AddMongoDB("mongo", 27017)
    .WithMongoExpress()
    .WithLifetime(ContainerLifetime.Persistent);

var mongodb = mongo.AddDatabase("filedb");

// Api
builder
    .AddProject<Projects.Microservice_Aspire_Api>("api")
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(messageBusEndpoint)
    .WaitFor(messageBus)
    .WithReference(storage)
    .WaitFor(storage)
    .WithReference(mongodb)
    .WaitFor(mongodb);

builder.Build().Run();
