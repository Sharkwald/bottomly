using Microsoft.Extensions.Logging;
using SlackNet.Interaction;
using SlackNet.WebApi;

namespace Bottomly.Slack;

public class PingSlashCommandHandler(ILogger<PingSlashCommandHandler> logger) : ISlashCommandHandler
{
    public Task<SlashCommandResponse> Handle(SlashCommand command)
    {
        logger.LogInformation("Received slash command: {Command}", command);
        return Task.FromResult(new SlashCommandResponse
        {
            Message = new Message { Text = "Pong!" },
            ResponseType = ResponseType.InChannel
        });
    }
}