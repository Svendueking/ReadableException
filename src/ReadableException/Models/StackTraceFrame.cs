namespace ReadableException.Models;

public class StackTraceFrame
{
    public string FullText { get; set; } = string.Empty;
    public string? MethodName { get; set; }
    public string? ClassName { get; set; }
    public string? Namespace { get; set; }
    public string? FileName { get; set; }
    public int? LineNumber { get; set; }
    public bool IsFiltered { get; set; }
    public bool IsHighlighted { get; set; }

    public string GetFullMethodName()
    {
        if (string.IsNullOrEmpty(ClassName) || string.IsNullOrEmpty(MethodName))
            return FullText;
        
        string ns = string.IsNullOrEmpty(Namespace) ? "" : $"{Namespace}.";
        return $"{ns}{ClassName}.{MethodName}";
    }
}
