namespace Microservice.Aspire.Api.Services;

using Microservice.Aspire.Api.Configurations;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

public class MongoDbService(
    IMongoClient mongoClient,
    IOptions<MongoDbSettings> mongoDbSettingsOptions)
{
    private readonly IMongoDatabase _database = mongoClient.GetDatabase(mongoDbSettingsOptions.Value.DatabaseName);

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
