namespace Microservice.Aspire.Api.Services;

public class PostgresService(
    ApiDbContext dbContext,
    ILogger<PostgresService> logger)
{
    private readonly ApiDbContext _dbContext = dbContext;
    private readonly ILogger<PostgresService> _logger = logger;
}
