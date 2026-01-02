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

        var exceptionInfo = _parser.Parse(exceptionText);
        if (exceptionInfo == null)
            return null;

        _filter.ApplyFilters(exceptionInfo);

        var result = new AnalysisResult
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
        var exceptionText = ExtractExceptionFromLog(logEntry);
        return Analyze(exceptionText);
    }

    private ExceptionInfo FindRootException(ExceptionInfo exception)
    {
        var current = exception;
        while (current.InnerException != null)
        {
            current = current.InnerException;
        }
        return current;
    }

    private List<ExceptionInfo> BuildExceptionChain(ExceptionInfo exception)
    {
        var chain = new List<ExceptionInfo>();
        var current = exception;
        
        while (current != null)
        {
            chain.Add(current);
            current = current.InnerException;
        }
        
        return chain;
    }

    private void CalculateStatistics(AnalysisResult result)
    {
        var totalFrames = 0;
        var visibleFrames = 0;
        
        foreach (var exception in result.ExceptionChain)
        {
            totalFrames += exception.StackTrace.Count;
            visibleFrames += exception.GetVisibleFrames().Count;
        }
        
        result.TotalFrames = totalFrames;
        result.VisibleFrames = visibleFrames;
        result.FilteredFrames = totalFrames - visibleFrames;
    }

    private void GenerateSummary(AnalysisResult result)
    {
        if (result.RootException == null)
        {
            result.Summary = "No exception found";
            return;
        }

        var highlightedFrames = result.RootException.GetHighlightedFrames();
        var summary = $"{result.RootException.ExceptionType}: {result.RootException.Message}";
        
        if (highlightedFrames.Any())
        {
            var firstHighlighted = highlightedFrames.First();
            summary += $" at {firstHighlighted.GetFullMethodName()}";
        }
        
        result.Summary = summary;
    }

    private string ExtractExceptionFromLog(string logEntry)
    {
        var lines = logEntry.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
        var exceptionLines = new List<string>();
        var inException = false;

        foreach (var line in lines)
        {
            if (line.Contains("Exception") || line.Trim().StartsWith("at "))
            {
                inException = true;
            }

            if (inException)
            {
                exceptionLines.Add(line);
                
                if (string.IsNullOrWhiteSpace(line) && exceptionLines.Count > 1)
                {
                    break;
                }
            }
        }

        return string.Join(Environment.NewLine, exceptionLines);
    }
}
