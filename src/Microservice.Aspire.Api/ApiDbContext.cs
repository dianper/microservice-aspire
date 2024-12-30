namespace Microservice.Aspire.Api;

using Microservice.Aspire.Api.Models;
using Microsoft.EntityFrameworkCore;

public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<GlobalSummaryModel> GlobalSummaries { get; set; }
}
