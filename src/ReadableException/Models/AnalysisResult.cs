namespace ReadableException.Models;

public class AnalysisResult
{
    public ExceptionInfo? RootException { get; set; }
    public List<ExceptionInfo> ExceptionChain { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
    public int TotalFrames { get; set; }
    public int VisibleFrames { get; set; }
    public int FilteredFrames { get; set; }

    public string ToFormattedString()
    {
        var sb = new System.Text.StringBuilder();
        
        if (RootException != null)
        {
            sb.AppendLine($"Root Cause: {RootException.ExceptionType}: {RootException.Message}");
            sb.AppendLine();
            
            var visibleFrames = RootException.GetVisibleFrames();
            if (visibleFrames.Any())
            {
                sb.AppendLine("Key Stack Frames:");
                foreach (var frame in visibleFrames)
                {
                    var marker = frame.IsHighlighted ? " [!]" : "";
                    sb.AppendLine($"  at {frame.GetFullMethodName()}{marker}");
                    if (!string.IsNullOrEmpty(frame.FileName) && frame.LineNumber.HasValue)
                    {
                        sb.AppendLine($"     in {frame.FileName}:line {frame.LineNumber}");
                    }
                }
            }
            
            sb.AppendLine();
            sb.AppendLine($"Statistics: {VisibleFrames} visible / {TotalFrames} total frames ({FilteredFrames} filtered)");
        }
        
        return sb.ToString();
    }
}
