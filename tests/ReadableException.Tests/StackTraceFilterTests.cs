using ReadableException.Configuration;
using ReadableException.Filtering;
using ReadableException.Models;
using Xunit;

namespace ReadableException.Tests;

public class StackTraceFilterTests
{
    [Fact]
    public void ApplyFilters_FilterFrameworkNamespaces_FiltersCorrectly()
    {
        var config = new AnalyzerConfiguration
        {
            FilteredNamespaces = new List<string> { "System." },
            FilterFrameworkCalls = true
        };
        var filter = new StackTraceFilter(config);

        var exceptionInfo = new ExceptionInfo
        {
            StackTrace = new List<StackTraceFrame>
            {
                new StackTraceFrame { Namespace = "System.Collections", ClassName = "List", MethodName = "Add" },
                new StackTraceFrame { Namespace = "MyApp", ClassName = "Service", MethodName = "Process" }
            }
        };

        filter.ApplyFilters(exceptionInfo);

        Assert.True(exceptionInfo.StackTrace[0].IsFiltered);
        Assert.False(exceptionInfo.StackTrace[1].IsFiltered);
    }

    [Fact]
    public void ApplyFilters_HighlightApplicationCode_HighlightsNonFramework()
    {
        var config = new AnalyzerConfiguration
        {
            FilteredNamespaces = new List<string> { "System." },
            HighlightApplicationCode = true
        };
        var filter = new StackTraceFilter(config);

        var exceptionInfo = new ExceptionInfo
        {
            StackTrace = new List<StackTraceFrame>
            {
                new StackTraceFrame { Namespace = "MyApp", ClassName = "Service", MethodName = "Process" }
            }
        };

        filter.ApplyFilters(exceptionInfo);

        Assert.True(exceptionInfo.StackTrace[0].IsHighlighted);
    }

    [Fact]
    public void ApplyFilters_FilteredFrames_NotHighlighted()
    {
        var config = new AnalyzerConfiguration
        {
            FilteredNamespaces = new List<string> { "System." },
            HighlightApplicationCode = true
        };
        var filter = new StackTraceFilter(config);

        var exceptionInfo = new ExceptionInfo
        {
            StackTrace = new List<StackTraceFrame>
            {
                new StackTraceFrame { Namespace = "System.Collections", ClassName = "List", MethodName = "Add" }
            }
        };

        filter.ApplyFilters(exceptionInfo);

        Assert.True(exceptionInfo.StackTrace[0].IsFiltered);
        Assert.False(exceptionInfo.StackTrace[0].IsHighlighted);
    }

    [Fact]
    public void ApplyFilters_CustomHighlightNamespace_HighlightsCorrectly()
    {
        var config = new AnalyzerConfiguration
        {
            HighlightedNamespaces = new List<string> { "MyApp.Core" }
        };
        var filter = new StackTraceFilter(config);

        var exceptionInfo = new ExceptionInfo
        {
            StackTrace = new List<StackTraceFrame>
            {
                new StackTraceFrame { Namespace = "MyApp.Core.Services", ClassName = "DataService", MethodName = "Load" }
            }
        };

        filter.ApplyFilters(exceptionInfo);

        Assert.True(exceptionInfo.StackTrace[0].IsHighlighted);
    }
}
