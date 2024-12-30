namespace Microservice.Aspire.Api.Models.Extensions;

public static class CovidGlobalDailyModelExtensions
{
    public static void SetIdentifier(this IEnumerable<CovidGlobalDailyModel> models, string identifier)
    {
        foreach (var model in models)
        {
            model.Identifier = identifier;
        }
    }
}
