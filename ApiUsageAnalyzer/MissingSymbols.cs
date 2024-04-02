namespace ApiUsageAnalyzer;

public record MissingSymbols(IReadOnlyCollection<string> Types, IReadOnlyCollection<string> Members);
