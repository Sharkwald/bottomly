using Bottomly.Configuration;
using Microsoft.Extensions.Options;

namespace Bottomly.Tests.Helpers;

internal static class TestHelpers
{
    public const string TestPrefix = "_";

    public static IOptions<BottomlyOptions> CreateOptions(string prefix = TestPrefix) =>
        Options.Create(new BottomlyOptions { Prefix = prefix });
}