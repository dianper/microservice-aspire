namespace Microservice.Aspire.Api.Services;

using Microservice.Aspire.Api.Models;
using Microsoft.EntityFrameworkCore;

public class PostgresService(
    ApiDbContext dbContext,
    ILogger<PostgresService> logger)
{
    private readonly ApiDbContext _dbContext = dbContext;
    private readonly ILogger<PostgresService> _logger = logger;

    public async Task<IEnumerable<GlobalSummaryModel>> GetGlobalSummariesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _dbContext.GlobalSummaries.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching records from Postgres");
            throw;
        }
    }

    public async Task InsertGlobalSummariesAsync(IEnumerable<GlobalSummaryModel> records, CancellationToken cancellationToken)
    {
        try
        {
            // Remove existing records to avoid duplicates
            _dbContext.GlobalSummaries.RemoveRange(_dbContext.GlobalSummaries);
            await _dbContext.GlobalSummaries.AddRangeAsync(records, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while inserting records to Postgres");
            throw;
        }
    }
}
