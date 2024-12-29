namespace Microservice.Aspire.Api.Models;

public record FileModel(Guid Identifier)
{
    public string? BlobUri { get; set; }

    public string? Name { get; set; }

    public string? Status { get; set; }
}
