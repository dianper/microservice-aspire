var builder = DistributedApplication.CreateBuilder(args);

var sqlPass = builder.AddParameter("password", "Password123!");

var sql = builder
    .AddSqlServer("sql", port: 58349, password: sqlPass)
    .WithLifetime(ContainerLifetime.Persistent);

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

var redis = builder
    .AddRedis("cache")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithRedisInsight();

builder
    .AddProject<Projects.Microservice_Aspire_Api>("api")
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(messageBusEndpoint)
    .WaitFor(messageBus);

builder.Build().Run();
