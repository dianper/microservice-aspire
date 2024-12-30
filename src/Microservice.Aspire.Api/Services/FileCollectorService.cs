namespace Microservice.Aspire.Api.Services;

using Microservice.Aspire.Api.Configurations;
using Microsoft.Extensions.Options;

public class FileCollectorService(
    FileUploaderClient fileUploaderClient,
    IOptions<FileCollectorSettings> fileSettingsOptions,
    ILogger<FileCollectorService> logger) : BackgroundService
{
    private readonly FileUploaderClient _fileUploaderClient = fileUploaderClient;
    private readonly FileCollectorSettings _fileSettings = fileSettingsOptions.Value;
    private readonly ILogger<FileCollectorService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait for 30 seconds before starting the message processing
        // This is to ensure that the service bus emulator is running
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        _logger.LogInformation("File collector service is running");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var files = Directory.GetFiles(_fileSettings.DirectoryPath, _fileSettings.SearchPattern, SearchOption.TopDirectoryOnly);

                foreach (var file in files)
                {
                    _logger.LogInformation($"Processing file: {file}");

                    // Upload the file
                    var result = await _fileUploaderClient.UploadAsync(file, stoppingToken);
                    if (result)
                    {
                        // Move the file to the processed folder
                        _logger.LogInformation("File uploaded successfully");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the files");
            }

            // Wait for 30 seconds before processing the next message
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
