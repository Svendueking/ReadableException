using ReadableException.Configuration;
using Xunit;

namespace ReadableException.Tests;

public class ExceptionAnalyzerTests
{
    [Fact]
    public void Analyze_ValidException_ReturnsAnalysisResult()
    {
        var analyzer = new ExceptionAnalyzer();
        var exceptionText = @"System.InvalidOperationException: Test exception
   at MyApp.Service.ProcessData() in C:\Projects\Service.cs:line 42
   at System.Threading.Tasks.Task.Execute()";

        var result = analyzer.Analyze(exceptionText);

        Assert.NotNull(result);
        Assert.NotNull(result.RootException);
        Assert.Equal("System.InvalidOperationException", result.RootException.ExceptionType);
        Assert.Single(result.ExceptionChain);
    }

    [Fact]
    public void Analyze_EmptyString_ReturnsNull()
    {
        var analyzer = new ExceptionAnalyzer();

        var result = analyzer.Analyze("");

        Assert.Null(result);
    }

    [Fact]
    public void Analyze_WithConfiguration_AppliesFilters()
    {
        var config = new ConfigurationBuilder()
            .FilterNamespace("System.")
            .HighlightNamespace("MyApp.")
            .Build();
        var analyzer = new ExceptionAnalyzer(config);
        
        var exceptionText = @"System.Exception: Test
   at System.Collections.List.Add()
   at MyApp.Service.Process()";

        var result = analyzer.Analyze(exceptionText);

        Assert.NotNull(result);
        Assert.True(result.FilteredFrames > 0);
    }

    [Fact]
    public void Analyze_CalculatesStatistics_Correctly()
    {
        var analyzer = new ExceptionAnalyzer();
        var exceptionText = @"System.Exception: Test
   at MyApp.Service.Method1()
   at MyApp.Service.Method2()
   at System.Threading.Tasks.Task.Run()";

        var result = analyzer.Analyze(exceptionText);

        Assert.NotNull(result);
        Assert.Equal(3, result.TotalFrames);
        Assert.True(result.VisibleFrames <= result.TotalFrames);
    }

    [Fact]
    public void Analyze_GeneratesSummary_Correctly()
    {
        var analyzer = new ExceptionAnalyzer();
        var exceptionText = @"System.InvalidOperationException: Operation failed
   at MyApp.Service.ProcessData()";

        var result = analyzer.Analyze(exceptionText);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Summary);
        Assert.Contains("InvalidOperationException", result.Summary);
    }

    [Fact]
    public void ToFormattedString_ProducesReadableOutput()
    {
        var analyzer = new ExceptionAnalyzer();
        var exceptionText = @"System.InvalidOperationException: Test exception
   at MyApp.Service.ProcessData()
   at MyApp.Controller.HandleRequest()";

        var result = analyzer.Analyze(exceptionText);

        Assert.NotNull(result);
        var formatted = result.ToFormattedString();
        Assert.NotEmpty(formatted);
        Assert.Contains("Root Cause:", formatted);
        Assert.Contains("InvalidOperationException", formatted);
    }
}
