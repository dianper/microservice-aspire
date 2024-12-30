namespace Microservice.Aspire.Api.Services;

using Microservice.Aspire.Api.Configurations;
using Microservice.Aspire.Api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

public class MongoDbService(
    IMongoClient mongoClient,
    IOptions<MongoDbSettings> mongoDbSettingsOptions)
{
    private readonly IMongoDatabase _database = mongoClient.GetDatabase(mongoDbSettingsOptions.Value.DatabaseName);

    public async Task<IEnumerable<GlobalSummaryModel>> GetGlobalSummaryAsync(string collectionName)
    {
        var pipeline = new EmptyPipelineDefinition<GlobalDetailsModel>()
            .Group(
                x => x.CountryName!,
                g => new GlobalSummaryModel
                {
                    Country = g.Key,
                    TotalNewCases = g.Sum(x => x.NewCases ?? 0),
                    TotalCumulativeCases = g.Sum(x => x.CumulativeCases ?? 0),
                    TotalNewDeaths = g.Sum(x => x.NewDeaths ?? 0),
                    TotalCumulativeDeaths = g.Sum(x => x.CumulativeDeaths ?? 0)
                }
            );

        var collection = _database.GetCollection<GlobalDetailsModel>(collectionName);

        return await collection.Aggregate(pipeline).ToListAsync();
    }

    public async Task<bool> ExistsAsync<T>(string collectionName, string column, string value)
    {
        var collection = _database.GetCollection<T>(collectionName);
        var filter = Builders<T>.Filter.Eq(column, value);
        return await collection.Find(filter).AnyAsync();
    }

    public async Task InsertManyAsync<T>(string collectionName, IEnumerable<T> records)
    {
        var collection = _database.GetCollection<T>(collectionName);
        await collection.InsertManyAsync(records);
    }
}
