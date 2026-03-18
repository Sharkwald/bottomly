namespace Bottomly.Commands.Search;

public abstract record SearchCommandResult;

public record SearchResult(string Title, string Link) : SearchCommandResult;

public record SearchApiErrorResult(string Error) : SearchCommandResult;

public record NoResultsFoundResult : SearchCommandResult;

public record EmptySearchTermErrorResult : SearchCommandResult;
