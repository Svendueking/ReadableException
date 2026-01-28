using ReadableException.Configuration;
using ReadableException.Filtering;
using ReadableException.Models;
using ReadableException.Parsing;

namespace ReadableException;

public class ExceptionAnalyzer
{
    private readonly AnalyzerConfiguration _configuration;
    private readonly ExceptionParser _parser;
    private readonly StackTraceFilter _filter;
    
    private static readonly char[] LineSeparators = { '\r', '\n' };

    public ExceptionAnalyzer() : this(AnalyzerConfiguration.Default)
    {
    }

    public ExceptionAnalyzer(AnalyzerConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _parser = new ExceptionParser();
        _filter = new StackTraceFilter(_configuration);
    }

    public AnalysisResult? Analyze(string exceptionText)
    {
        if (string.IsNullOrWhiteSpace(exceptionText))
            return null;

        ExceptionInfo? exceptionInfo = ExceptionParser.Parse(exceptionText);
        if (exceptionInfo == null)
            return null;

        _filter.ApplyFilters(exceptionInfo);

        AnalysisResult result = new()
        {
            RootException = FindRootException(exceptionInfo),
            ExceptionChain = BuildExceptionChain(exceptionInfo)
        };

        CalculateStatistics(result);
        GenerateSummary(result);

        return result;
    }

    public AnalysisResult? AnalyzeFromLog(string logEntry)
    {
        string exceptionText = ExtractExceptionFromLog(logEntry);
        return Analyze(exceptionText);
    }

    private static ExceptionInfo FindRootException(ExceptionInfo exception)
    {
        ExceptionInfo current = exception;
        while (current.InnerException != null)
        {
            current = current.InnerException;
        }
        return current;
    }

    private static List<ExceptionInfo> BuildExceptionChain(ExceptionInfo exception)
    {
        List<ExceptionInfo> chain = [];
        ExceptionInfo? current = exception;
        
        while (current != null)
        {
            chain.Add(current);
            current = current.InnerException;
        }
        
        return chain;
    }

    private static void CalculateStatistics(AnalysisResult result)
    {
        int totalFrames = 0;
        int visibleFrames = 0;
        
        foreach (ExceptionInfo exception in result.ExceptionChain)
        {
            totalFrames += exception.StackTrace.Count;
            visibleFrames += exception.GetVisibleFrames().Count;
        }
        
        result.TotalFrames = totalFrames;
        result.VisibleFrames = visibleFrames;
        result.FilteredFrames = totalFrames - visibleFrames;
    }

    private static void GenerateSummary(AnalysisResult result)
    {
        if (result.RootException == null)
        {
            result.Summary = "No exception found";
            return;
        }

        List<StackTraceFrame> highlightedFrames = result.RootException.GetHighlightedFrames();
        string summary = $"{result.RootException.ExceptionType}: {result.RootException.Message}";
        
        if (highlightedFrames.Count != 0)
        {
            StackTraceFrame firstHighlighted = highlightedFrames.First();
            summary += $" at {firstHighlighted.GetFullMethodName()}";
        }
        
        result.Summary = summary;
    }

    private static string ExtractExceptionFromLog(string logEntry)
    {
        string[] lines = logEntry.Split(LineSeparators, StringSplitOptions.None);
        List<string> exceptionLines = [];
        bool inException = false;

        foreach (string line in lines)
        {
            // Check if this line starts the exception section
            if (line.Contains("Exception:") || line.Trim().StartsWith("at ", StringComparison.Ordinal))
            {
                inException = true;
            }

            if (inException)
            {
                // Strip "Exception: " prefix if it exists at the start
                string processedLine = line;
                if (line.Trim().StartsWith("Exception:", StringComparison.Ordinal))
                {
                    int colonIndex = line.IndexOf("Exception:", StringComparison.Ordinal);
                    processedLine = line.Substring(colonIndex + "Exception:".Length).Trim();
                }
                
                exceptionLines.Add(processedLine);
                
                if (string.IsNullOrWhiteSpace(line) && exceptionLines.Count > 1)
                {
                    break;
                }
            }
        }

        return string.Join(Environment.NewLine, exceptionLines);
    }
}
