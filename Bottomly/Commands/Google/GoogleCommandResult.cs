namespace Bottomly.Commands.Google;

public abstract record GoogleCommandResult;

public record GoogleSearchResult(string Title, string Link) : GoogleCommandResult;

public record GoogleApiErrorResult(string Error) : GoogleCommandResult;

public record NoResultsFoundResult : GoogleCommandResult;

public record EmptySearchTermErrorResult : GoogleCommandResult;
