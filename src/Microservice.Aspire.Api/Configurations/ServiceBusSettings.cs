namespace Microservice.Aspire.Api.Configurations;

public record ServiceBusSettings
{
    public string GlobalDetailsQueue { get; set; }

    public string GlobalSummaryQueue { get; set; }
}
