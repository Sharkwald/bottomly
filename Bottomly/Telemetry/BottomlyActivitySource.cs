using System.Diagnostics;

namespace Bottomly.Telemetry;

internal static class BottomlyActivitySource
{
    internal const string Name = "Bottomly";
    internal static readonly ActivitySource Instance = new(Name);
}
