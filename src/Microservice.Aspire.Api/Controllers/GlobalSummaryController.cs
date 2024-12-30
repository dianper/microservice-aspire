namespace Microservice.Aspire.Api.Controllers;

using Asp.Versioning;
using Microservice.Aspire.Api.Constants;
using Microservice.Aspire.Api.Models;
using Microservice.Aspire.Api.Services;
using Microsoft.AspNetCore.Mvc;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{apiVersion}/summaries")]
public class GlobalSummaryController(
    PostgresService postgresService,
    RedisService redisService,
    ILogger<GlobalSummaryController> logger) : ControllerBase
{
    private readonly PostgresService _postgresService = postgresService;
    private readonly RedisService _redisService = redisService;

    private readonly ILogger<GlobalSummaryController> _logger = logger;

    [HttpGet]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        // Check if the data is available in Redis
        var summaries = await _redisService.GetAsync<IEnumerable<GlobalSummaryModel>>(RedisConstants.GlobalSummaryCacheKey);
        if (summaries is null)
        {
            // Fetch the data from Postgres
            summaries = await _postgresService.GetGlobalSummariesAsync(cancellationToken);
            if (!summaries.Any())
            {
                _logger.LogInformation("No records found in Postgres");

                return Ok(new
                {
                    Data = Enumerable.Empty<GlobalSummaryModel>(),
                    Message = "No records found"
                });
            }

            await _redisService.SetAsync(RedisConstants.GlobalSummaryCacheKey, summaries);
        }

        return Ok(new
        {
            Data = summaries,
            Message = string.Empty
        });
    }
}
