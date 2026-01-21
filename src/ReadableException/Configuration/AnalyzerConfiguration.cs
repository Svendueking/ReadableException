namespace ReadableException.Configuration;

public class AnalyzerConfiguration
{
    public List<string> FilteredNamespaces { get; set; } = new()
    {
        "System.",
        "Microsoft.Extensions.",
        "Microsoft.AspNetCore."
    };

    public List<string> HighlightedNamespaces { get; set; } = new();
    public List<string> FilteredClassNames { get; set; } = new();
    public List<string> HighlightedClassNames { get; set; } = new();
    public bool FilterFrameworkCalls { get; set; } = true;
    public bool HighlightApplicationCode { get; set; } = true;
    public int MaxStackFrames { get; set; } = 50;
    public bool ShowFileInfo { get; set; } = true;

    public static AnalyzerConfiguration Default => new();
}
