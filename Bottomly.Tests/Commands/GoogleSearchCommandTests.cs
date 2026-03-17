using Bottomly.Commands;
using Bottomly.Configuration;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Bottomly.Tests.Commands;

public class GoogleSearchCommandTests
{
    [Fact]
    public async Task ExecuteAsync_EmptyInput_ReturnsNull()
    {
        var options = Options.Create(new BottomlyOptions { GoogleApiKey = "key", GoogleCseId = "cse" });
        var command = new GoogleSearchCommand(options);

        var result = await command.ExecuteAsync("");

        result.ShouldBeNull();
    }
}