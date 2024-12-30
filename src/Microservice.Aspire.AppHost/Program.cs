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
var mongoUser = builder.AddParameter("mongo-user", "user1");
var mongoPass = builder.AddParameter("mongo-password", "123456", secret: true);

var mongo = builder
    .AddMongoDB("mongo", 27017, mongoUser, mongoPass)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithMongoExpress();

var mongodb = mongo
    .AddDatabase("mongodb");

// Postgres
var pgUser = builder.AddParameter("pg-user", "user1");
var pgPass = builder.AddParameter("pg-password", "123456");

var postgres = builder
    .AddPostgres("postgres", userName: pgUser, password: pgPass, port: 58198)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin()
    .WithPgWeb();

var postgresdb = postgres.AddDatabase("postgresdb");

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
    .WaitFor(mongodb)
    .WithReference(postgresdb)
    .WaitFor(postgresdb);

builder.Build().Run();
