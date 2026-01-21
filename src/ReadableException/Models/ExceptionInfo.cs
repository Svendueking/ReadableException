namespace ReadableException.Models;

public class ExceptionInfo
{
    public string ExceptionType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<StackTraceFrame> StackTrace { get; set; } = new();
    public ExceptionInfo? InnerException { get; set; }
    public string RawText { get; set; } = string.Empty;

    public List<StackTraceFrame> GetVisibleFrames()
    {
        return StackTrace.Where(f => !f.IsFiltered).ToList();
    }

    public List<StackTraceFrame> GetHighlightedFrames()
    {
        return StackTrace.Where(f => f.IsHighlighted).ToList();
    }
}
