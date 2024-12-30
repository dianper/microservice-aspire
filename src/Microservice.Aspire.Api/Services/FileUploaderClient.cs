namespace Microservice.Aspire.Api.Services;

using System.Net.Http.Headers;

public class FileUploaderClient(
    HttpClient httpClient,
    ILogger<FileUploaderClient> logger)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<FileUploaderClient> _logger = logger;

    public async Task<bool> UploadAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            using var formContent = new MultipartFormDataContent();
            await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            // Add the file to the form content
            formContent.Add(fileContent, "file", Path.GetFileName(filePath));

            // Send the request
            var response = await _httpClient.PostAsync("/api/v1/files", formContent, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("An error occurred while uploading the file");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while uploading the file");
            return false;
        }
    }
}
