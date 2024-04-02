namespace ApiUsageAnalyzer;

public record InputAssembly(string? FileName)
{
    public IList<string>? SearchPaths { get; set; } = [];
}
