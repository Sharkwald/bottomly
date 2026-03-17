namespace Bottomly.Configuration;

public class BottomlyOptions
{
    public string SlackBotToken { get; set; } = string.Empty;
    public string SlackAppToken { get; set; } = string.Empty;
    public string GoogleApiKey { get; set; } = string.Empty;
    public string GoogleCseId { get; set; } = string.Empty;
    public string Prefix { get; set; } = "!";
    public string GiphyApiKey { get; set; } = string.Empty;
    public string Environment { get; set; } = "live";
    public string GitHubToken { get; set; } = string.Empty;

    public bool IsDebug => Environment != "live";
}