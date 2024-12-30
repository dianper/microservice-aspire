namespace Microservice.Aspire.Api.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("globalsummaries")]
public class GlobalSummaryModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public string Country { get; set; }

    public long TotalNewCases { get; set; }

    public long TotalCumulativeCases { get; set; }

    public long TotalNewDeaths { get; set; }

    public long TotalCumulativeDeaths { get; set; }
}
