using NUnit.Framework;
using ReadableException.Configuration;

namespace ReadableException.Tests;

[TestFixture]
public class ExceptionAnalyzerTests
{
    [Test]
    public void Analyze_ValidException_ReturnsAnalysisResult()
    {
        ExceptionAnalyzer analyzer = new ExceptionAnalyzer();
        string exceptionText = @"System.InvalidOperationException: Test exception
   at MyApp.Service.ProcessData() in C:\Projects\Service.cs:line 42
   at System.Threading.Tasks.Task.Execute()";

        ReadableException.Models.AnalysisResult? result = analyzer.Analyze(exceptionText);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.RootException, Is.Not.Null);
        Assert.That(result.RootException!.ExceptionType, Is.EqualTo("System.InvalidOperationException"));
        Assert.That(result.ExceptionChain.Count, Is.EqualTo(1));
    }

    [Test]
    public void Analyze_EmptyString_ReturnsNull()
    {
        ExceptionAnalyzer analyzer = new ExceptionAnalyzer();

        ReadableException.Models.AnalysisResult? result = analyzer.Analyze("");

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Analyze_WithConfiguration_AppliesFilters()
    {
        AnalyzerConfiguration config = new ConfigurationBuilder()
            .FilterNamespace("System.")
            .HighlightNamespace("MyApp.")
            .Build();
        ExceptionAnalyzer analyzer = new ExceptionAnalyzer(config);
        
        string exceptionText = @"System.Exception: Test
   at System.Collections.List.Add()
   at MyApp.Service.Process()";

        ReadableException.Models.AnalysisResult? result = analyzer.Analyze(exceptionText);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.FilteredFrames, Is.GreaterThan(0));
    }

    [Test]
    public void Analyze_CalculatesStatistics_Correctly()
    {
        ExceptionAnalyzer analyzer = new ExceptionAnalyzer();
        string exceptionText = @"System.Exception: Test
   at MyApp.Service.Method1()
   at MyApp.Service.Method2()
   at System.Threading.Tasks.Task.Run()";

        ReadableException.Models.AnalysisResult? result = analyzer.Analyze(exceptionText);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.TotalFrames, Is.EqualTo(3));
        Assert.That(result.VisibleFrames, Is.LessThanOrEqualTo(result.TotalFrames));
    }

    [Test]
    public void Analyze_GeneratesSummary_Correctly()
    {
        ExceptionAnalyzer analyzer = new ExceptionAnalyzer();
        string exceptionText = @"System.InvalidOperationException: Operation failed
   at MyApp.Service.ProcessData()";

        ReadableException.Models.AnalysisResult? result = analyzer.Analyze(exceptionText);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Summary, Is.Not.Empty);
        Assert.That(result.Summary, Does.Contain("InvalidOperationException"));
    }

    [Test]
    public void ToFormattedString_ProducesReadableOutput()
    {
        ExceptionAnalyzer analyzer = new ExceptionAnalyzer();
        string exceptionText = @"System.InvalidOperationException: Test exception
   at MyApp.Service.ProcessData()
   at MyApp.Controller.HandleRequest()";

        ReadableException.Models.AnalysisResult? result = analyzer.Analyze(exceptionText);

        Assert.That(result, Is.Not.Null);
        string formatted = result!.ToFormattedString();
        Assert.That(formatted, Is.Not.Empty);
        Assert.That(formatted, Does.Contain("Root Cause:"));
        Assert.That(formatted, Does.Contain("InvalidOperationException"));
    }
}
