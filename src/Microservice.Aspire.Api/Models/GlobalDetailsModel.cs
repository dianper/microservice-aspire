namespace Microservice.Aspire.Api.Models;

using CsvHelper.Configuration.Attributes;

public class GlobalDetailsModel
{
    [Index(0)]
    public string? DateReported { get; set; }

    [Index(1)]
    public string? CountryCode { get; set; }

    [Index(2)]
    public string? CountryName { get; set; }

    [Index(3)]
    public string? WhoRegion { get; set; }

    [Index(4)]
    public int? NewCases { get; set; }

    [Index(5)]
    public int? CumulativeCases { get; set; }

    [Index(6)]
    public int? NewDeaths { get; set; }

    [Index(7)]
    public int? CumulativeDeaths { get; set; }

    [Ignore]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Ignore]
    public string? Identifier { get; set; }

    [Ignore]
    public string? FileName { get; set; }
}
