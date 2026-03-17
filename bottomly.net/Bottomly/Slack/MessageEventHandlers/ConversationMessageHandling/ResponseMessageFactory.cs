using Bottomly.LlmBot;

namespace Bottomly.Slack.MessageEventHandlers.ConversationMessageHandling;

public static class ResponseMessageFactory
{
    private static readonly string[] TimeoutMessages =
    [
        "I must apologise, in seeking an answer I appear to have taken rather too long. Perhaps another attempt?",
        "I'm afraid I've been away for a while, but I may be able to assist now. Please try again.",
        "I find myself in the regrettable position of having exceeded the allotted time. If you would be so good as to try once more, I shall endeavour to be more expeditious.",
        "My most sincere apologies, I appear to have wandered into something of a reverie. Might I trouble you to repeat the enquiry?",
        "I fear I have been somewhat remiss in my promptness. A thousand pardons; do please try again.",
        "It is with considerable chagrin that I must report a delay of the most unfortunate variety. Another attempt, if you would be so kind.",
        "I confess the matter proved rather more taxing than anticipated, and I have overstayed my welcome. Pray, try again and I shall not dally.",
        "One does not like to make excuses, but the cerebral machinery appears to have momentarily seized. I am ready to try anew, should you wish it.",
        "I am mortified to report that I have taken an unconscionable time about it. Please do try once more, I shall be the very soul of alacrity.",
        "The wheels of cogitation were, I regret to say, turning at an altogether insufficient pace. Another enquiry, at your convenience, and I shall apply myself with renewed vigour."
    ];

    private static readonly string[] UsageExceededMessages =
    [
        "I am afraid, sir, that the well of available queries has run rather dry for the present period. One must, regrettably, wait until it replenishes itself.",
        "It pains me to inform you that we have exhausted our allocation for the time being. The situation will, I trust, resolve itself in due course.",
        "I find myself in the unenviable position of having nothing further to offer at this juncture — the usage limit has been reached. Patience, if you please.",
        "One hesitates to disappoint, but the monthly ration of queries has, I fear, been fully consumed. Normal service will be resumed presently.",
        "I must beg your indulgence: the permitted number of requests has been reached. A brief interval, and we shall be back on form.",
        "It is with a heavy heart that I report all available capacity has been spoken for. One will simply have to bide one's time.",
        "The cupboard, as it were, is bare — at least insofar as remaining usage is concerned. I recommend patience and, perhaps, a restorative cup of tea.",
        "I regret that the quota has been exhausted. These bureaucratic constraints are, I admit, most vexing, but there it is.",
        "Usage limits, much like the patience of one's employer, are not inexhaustible. We appear to have reached ours. Kindly try again later.",
        "I am compelled to inform you that further assistance must await the next allocation period. One does not make the rules, one merely observes them."
    ];


    private static readonly string[] UnknownErrorMessage =
    [
        "I find myself at something of a loss, sir. An error of an unspecified nature has occurred. Might I suggest trying again?",
        "Something has gone wrong, though I confess I cannot precisely identify what. I shall merely note that it was not, in any meaningful sense, intentional.",
        "I am afraid an unforeseen difficulty has presented itself. These things happen, even to the most well-ordered of systems.",
        "An error has arisen, the precise variety remains unclear, but I should not allow it to dampen one's spirits unduly. Please do try again.",
        "I find this most vexing. Something has gone awry, and I am not entirely certain what. Your patience, as ever, is most appreciated.",
        "Something in the works appears to have come a bit unstuck. I offer my apologies and strongly recommend another attempt.",
        "I cannot account for what has transpired, but I can confirm it was not the intended outcome. Shall we try once more?",
        "An unexpected snag has arisen, not, I hasten to add, through any want of effort on my part. Please try again, and I shall do better.",
        "The situation is, I regret to say, unclear. An error has manifested itself, and I am taking steps to look appropriately abashed about it.",
        "I am as surprised as you are, though I shall endeavour not to show it. Something has misfired. Another attempt, if you would be so kind."
    ];

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