namespace Microservice.Aspire.Api.Models;

public record FileModel(string Identifier)
{
    public string? BlobUri { get; set; }

    public string? Name { get; set; }

    public string? Status { get; set; }
}
