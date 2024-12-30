namespace Microservice.Aspire.Api.Models.Extensions;

public static class CovidGlobalDailyModelExtensions
{
    public static void SetIdentifiers(this IEnumerable<GlobalDetailsModel> models, string identifier, string fileName)
    {
        foreach (var model in models)
        {
            model.Identifier = identifier;
            model.FileName = fileName;
        }
    }
}
