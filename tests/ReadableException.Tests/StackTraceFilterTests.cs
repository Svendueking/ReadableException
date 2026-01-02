using NUnit.Framework;
using ReadableException.Configuration;
using ReadableException.Filtering;
using ReadableException.Models;

namespace ReadableException.Tests;

[TestFixture]
public class StackTraceFilterTests
{
    [Test]
    public void ApplyFilters_FilterFrameworkNamespaces_FiltersCorrectly()
    {
        AnalyzerConfiguration config = new AnalyzerConfiguration
        {
            FilteredNamespaces = new List<string> { "System." },
            FilterFrameworkCalls = true
        };
        StackTraceFilter filter = new StackTraceFilter(config);

        ExceptionInfo exceptionInfo = new ExceptionInfo
        {
            StackTrace = new List<StackTraceFrame>
            {
                new StackTraceFrame { Namespace = "System.Collections", ClassName = "List", MethodName = "Add" },
                new StackTraceFrame { Namespace = "MyApp", ClassName = "Service", MethodName = "Process" }
            }
        };

        filter.ApplyFilters(exceptionInfo);

        Assert.That(exceptionInfo.StackTrace[0].IsFiltered, Is.True);
        Assert.That(exceptionInfo.StackTrace[1].IsFiltered, Is.False);
    }

    [Test]
    public void ApplyFilters_HighlightApplicationCode_HighlightsNonFramework()
    {
        AnalyzerConfiguration config = new AnalyzerConfiguration
        {
            FilteredNamespaces = new List<string> { "System." },
            HighlightApplicationCode = true
        };
        StackTraceFilter filter = new StackTraceFilter(config);

        ExceptionInfo exceptionInfo = new ExceptionInfo
        {
            StackTrace = new List<StackTraceFrame>
            {
                new StackTraceFrame { Namespace = "MyApp", ClassName = "Service", MethodName = "Process" }
            }
        };

        filter.ApplyFilters(exceptionInfo);

        Assert.That(exceptionInfo.StackTrace[0].IsHighlighted, Is.True);
    }

    [Test]
    public void ApplyFilters_FilteredFrames_NotHighlighted()
    {
        AnalyzerConfiguration config = new AnalyzerConfiguration
        {
            FilteredNamespaces = new List<string> { "System." },
            HighlightApplicationCode = true
        };
        StackTraceFilter filter = new StackTraceFilter(config);

        ExceptionInfo exceptionInfo = new ExceptionInfo
        {
            StackTrace = new List<StackTraceFrame>
            {
                new StackTraceFrame { Namespace = "System.Collections", ClassName = "List", MethodName = "Add" }
            }
        };

        filter.ApplyFilters(exceptionInfo);

        Assert.That(exceptionInfo.StackTrace[0].IsFiltered, Is.True);
        Assert.That(exceptionInfo.StackTrace[0].IsHighlighted, Is.False);
    }

    [Test]
    public void ApplyFilters_CustomHighlightNamespace_HighlightsCorrectly()
    {
        AnalyzerConfiguration config = new AnalyzerConfiguration
        {
            HighlightedNamespaces = new List<string> { "MyApp.Core" }
        };
        StackTraceFilter filter = new StackTraceFilter(config);

        ExceptionInfo exceptionInfo = new ExceptionInfo
        {
            StackTrace = new List<StackTraceFrame>
            {
                new StackTraceFrame { Namespace = "MyApp.Core.Services", ClassName = "DataService", MethodName = "Load" }
            }
        };

        filter.ApplyFilters(exceptionInfo);

        Assert.That(exceptionInfo.StackTrace[0].IsHighlighted, Is.True);
    }
}
