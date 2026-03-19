using Bottomly.Configuration;
using Bottomly.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackNet;
using SlackNet.Blocks;
using SlackNet.WebApi;

namespace Bottomly.Slack;

public class SlackMessageBroker(
    IMemberRepository repository,
    ISlackApiClient slack,
    IOptions<BottomlyOptions> options,
    ILogger<SlackMessageBroker> logger)
    : ISlackMessageBroker
{
    private readonly BottomlyOptions _options = options.Value;

    public async Task SendMessageAsync(string text, string channel, string? replyToTs = null)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        if (_options.IsDebug)
        {
            text = $"[{_options.Environment}] {text}";
        }

        try
        {
            var message = new Message
            {
                Channel = channel,
                Text = text,
                ThreadTs = replyToTs
            };
            await slack.Chat.PostMessage(message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending message to channel {Channel}", channel);
        }
    }

    public async Task SendBlocksMessageAsync(IReadOnlyList<Block> blocks, string channel, string? text = null, string? replyToTs = null)
    {
        try
        {
            var message = new Message
            {
                Channel = channel,
                Text = _options.IsDebug && text is not null ? $"[{_options.Environment}] {text}" : text ?? string.Empty,
                Blocks = [..blocks],
                ThreadTs = replyToTs
            };
            await slack.Chat.PostMessage(message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending blocks message to channel {Channel}", channel);
        }
    }

    public async Task SendReactionAsync(string emoji, string channel, string timestamp)
    {
        try
        {
            await slack.Reactions.AddToMessage(emoji, channel, timestamp);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending reaction to channel {Channel}", channel);
        }
    }

    public async Task SendDmAsync(string text, string username)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        try
        {
            var member = await repository.GetByUsernameAsync(username);
            var channelId = await slack.Conversations.Open([member!.SlackId]);
            await SendMessageAsync(text, channelId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending DM to user {UserId}", username);
        }
    }
}