namespace Microservice.Aspire.Api.Services;

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microservice.Aspire.Api.Services.Responses;

public class AzureBlobStorageService(
    BlobServiceClient blobServiceClient,
    ILogger<AzureBlobStorageService> logger)
{
    private readonly BlobServiceClient _blobServiceClient = blobServiceClient;
    private readonly ILogger<AzureBlobStorageService> _logger = logger;

    public async Task<AzureBlobStorageResponse> DownloadAsync(string fileName, CancellationToken cancellationToken)
    {
        try
        {
            var container = _blobServiceClient.GetBlobContainerClient("files");

            var blobClient = container.GetBlobClient(fileName);

            var memory = new MemoryStream();

            await blobClient.DownloadToAsync(memory, cancellationToken);

            return AzureBlobStorageResponse.Success("File downloaded successfully", memory.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while downloading the file");

            return AzureBlobStorageResponse.Failure(ex);
        }
    }

    public async Task<AzureBlobStorageResponse> UploadAsync(IFormFile file, CancellationToken cancellationToken)
    {
        try
        {
            var fileName = file.FileName;

            var blobContainerClient = _blobServiceClient.GetBlobContainerClient("files");

            await blobContainerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            var blobClient = blobContainerClient.GetBlobClient(fileName);

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(
                content: stream,
                httpHeaders: new BlobHttpHeaders { ContentType = file.ContentType },
                cancellationToken: cancellationToken);

            return AzureBlobStorageResponse.Success("File uploaded successfully", blobClient.Uri.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while uploading the file");

            return AzureBlobStorageResponse.Failure(ex);
        }
    }
}
