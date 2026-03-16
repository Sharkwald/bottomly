using Bottomly.LlmBot;

namespace Bottomly.Slack.MessageEventHandlers.ConversationMessageHandling;

public static class ResponseMessageFactory
{
    private static readonly string[] TimeoutMessages =
    [
        "I must apologise, in seeking an answer I appear to have taken rather too long. Perhaps another attempt?",
        "I'm afraid I've been away for a while, but I may be able to assist now. Please try again."
    ];

    private static readonly string[] UsageExceededMessages =
        ["Sorry, I've reached my limit for today. Please try again tomorrow."];


    private static readonly string[] UnknownErrorMessage =
        ["Sorry, I'm having trouble understanding your request. Please try again."];

    public static string ToSlackResponse(this LlmResponse llmResponse) =>
        llmResponse switch
        {
            LlmMessageResponse success => success.Message,
            LlmTimeoutResponse => ToTimeoutMessage(),
            LlmUsageExceededResponse => ToUsageExceededMessage(),
            _ => ToUnknownErrorMessage()
        };

    private static string ToTimeoutMessage() => TimeoutMessages.Shuffle().First();

    private static string ToUsageExceededMessage() => UsageExceededMessages.Shuffle().First();

    private static string ToUnknownErrorMessage() => UnknownErrorMessage.Shuffle().First();

    private static T[] Shuffle<T>(this T[] array) => array.OrderBy(_ => Random.Shared.Next()).ToArray();
}