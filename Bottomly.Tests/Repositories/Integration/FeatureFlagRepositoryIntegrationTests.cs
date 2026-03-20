using Bottomly.Repositories;
using Bottomly.Tests.Infrastructure;
using MongoDB.Driver;
using Shouldly;

namespace Bottomly.Tests.Repositories.Integration;

[Collection("MongoDB")]
public class FeatureFlagRepositoryIntegrationTests(MongoDbFixture fixture) : IAsyncLifetime
{
    private IMongoDatabase _db = null!;
    private FeatureFlagRepository _sut = null!;

    public Task InitializeAsync()
    {
        _db = fixture.GetDatabase($"flags_test_{Guid.NewGuid():N}");
        _sut = new FeatureFlagRepository(_db);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync() =>
        await fixture.Client.DropDatabaseAsync(_db.DatabaseNamespace.DatabaseName);

    // ─── GetAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAsync_WhenFlagDoesNotExist_ReturnsFalse()
    {
        var result = await _sut.GetAsync("EnableLlm");

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task GetAsync_WhenFlagIsTrue_ReturnsTrue()
    {
        await _sut.SetAsync("EnableLlm", true);

        var result = await _sut.GetAsync("EnableLlm");

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task GetAsync_WhenFlagIsFalse_ReturnsFalse()
    {
        await _sut.SetAsync("EnableLlm", false);

        var result = await _sut.GetAsync("EnableLlm");

        result.ShouldBeFalse();
    }

    // ─── SetAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task SetAsync_CreatesDocumentWhenItDoesNotExist()
    {
        await _sut.SetAsync("EnableLlm", true);

        var result = await _sut.GetAsync("EnableLlm");
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task SetAsync_UpdatesExistingDocument()
    {
        await _sut.SetAsync("EnableLlm", true);
        await _sut.SetAsync("EnableLlm", false);

        var result = await _sut.GetAsync("EnableLlm");
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task SetAsync_ToggleOnThenOff_ReturnsCorrectValue()
    {
        await _sut.SetAsync("EnableLlm", false);
        await _sut.SetAsync("EnableLlm", true);
        await _sut.SetAsync("EnableLlm", false);

        var result = await _sut.GetAsync("EnableLlm");
        result.ShouldBeFalse();
    }

    // ─── SeedAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task SeedAsync_WhenFlagDoesNotExist_CreatesWithDefaultValue()
    {
        await _sut.SeedAsync("EnableLlm", true);

        var result = await _sut.GetAsync("EnableLlm");
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task SeedAsync_WhenFlagAlreadyExists_DoesNotOverwrite()
    {
        await _sut.SetAsync("EnableLlm", true);

        await _sut.SeedAsync("EnableLlm", false);

        var result = await _sut.GetAsync("EnableLlm");
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task SeedAsync_CalledMultipleTimes_Idempotent()
    {
        await _sut.SeedAsync("EnableLlm", false);
        await _sut.SeedAsync("EnableLlm", true);
        await _sut.SeedAsync("EnableLlm", true);

        var result = await _sut.GetAsync("EnableLlm");
        result.ShouldBeFalse();
    }

    // ─── Multiple flags ────────────────────────────────────────────────────────

    [Fact]
    public async Task SetAsync_DifferentFlagIds_AreStoredIndependently()
    {
        await _sut.SetAsync("EnableLlm", true);
        await _sut.SetAsync("AnotherFlag", false);

        (await _sut.GetAsync("EnableLlm")).ShouldBeTrue();
        (await _sut.GetAsync("AnotherFlag")).ShouldBeFalse();
    }
}