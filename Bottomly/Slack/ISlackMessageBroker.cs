using SlackNet.Blocks;

namespace Bottomly.Slack;

public interface ISlackMessageBroker
{
    Task SendMessageAsync(string text, string channel, string? replyToTs = null);
    Task SendBlocksMessageAsync(IReadOnlyList<Block> blocks, string channel, string? text = null, string? replyToTs = null);
    Task SendReactionAsync(string emoji, string channel, string timestamp);
    Task SendDmAsync(string text, string username);
}