namespace Microservice.Aspire.Api.Configurations;

public record FileCollectorSettings
{
    public IEnumerable<string> AllowedExtensions { get; set; }

    public string DirectoryPath { get; set; }

    public string SearchPattern { get; set; }
}
