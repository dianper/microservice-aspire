namespace Microservice.Aspire.Api.Configurations;

public record MongoDbSettings
{
    public string DatabaseName { get; set; }

    public string GlobalDetailsCollection { get; set; }

    public string GlobalSummaryCollection { get; set; }
}
