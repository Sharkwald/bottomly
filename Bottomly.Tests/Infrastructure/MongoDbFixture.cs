using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace Bottomly.Tests.Infrastructure;

/// <summary>
/// Shared xUnit fixture that starts a single MongoDB container for the entire test collection.
/// Each test class should call <see cref="GetDatabase"/> with a unique name to ensure isolation.
/// </summary>
public sealed class MongoDbFixture : IAsyncLifetime
{
    private readonly MongoDbContainer _container = new MongoDbBuilder("mongo:8")
        .Build();

    public IMongoClient Client { get; private set; } = null!;

    public IMongoDatabase GetDatabase(string name) => Client.GetDatabase(name);

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        Client = new MongoClient(_container.GetConnectionString());
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();
}
