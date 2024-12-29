namespace Microservice.Aspire.Api.Services;

using MongoDB.Driver;

public class MongoDbService(IMongoClient mongoClient)
{
    private readonly IMongoClient _mongoClient = mongoClient;

    public async Task SaveManyAsync<T>(string databaseName, string collectionName, IEnumerable<T> records)
    {
        var database = _mongoClient.GetDatabase(databaseName);
        var collection = database.GetCollection<T>(collectionName);

        await collection.DeleteManyAsync(Builders<T>.Filter.Empty);
        await collection.InsertManyAsync(records);
    }
}
