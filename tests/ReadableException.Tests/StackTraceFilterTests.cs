using NUnit.Framework;
using ReadableException.Configuration;
using ReadableException.Filtering;
using ReadableException.Models;

namespace ReadableException.Tests;

[TestFixture]
public class StackTraceFilterTests
{
[Test]
    public void ApplyFiltersFilterFrameworkNamespacesFiltersCorrectly()
    {
AnalyzerConfiguration config = new()
        {
            FilteredNamespaces = new() { "System." },
            FilterFrameworkCalls = true
        };
        StackTraceFilter filter = new(config);

        ExceptionInfo exceptionInfo = new()
        {
            StackTrace = new()
            {
                new() { Namespace = "System.Collections", ClassName = "List", MethodName = "Add" },
                new() { Namespace = "MyApp", ClassName = "Service", MethodName = "Process" }
            }
        };

        filter.ApplyFilters(exceptionInfo);

        Assert.That(exceptionInfo.StackTrace[0].IsFiltered, Is.True);
        Assert.That(exceptionInfo.StackTrace[1].IsFiltered, Is.False);
    }

[Test]
    public void ApplyFiltersHighlightApplicationCodeHighlightsNonFramework()
    {
        AnalyzerConfiguration config = new()
        {
            FilteredNamespaces = new() { "System." },
            HighlightApplicationCode = true
        };
        StackTraceFilter filter = new(config);

        ExceptionInfo exceptionInfo = new()
        {
            StackTrace = new()
            {
                new() { Namespace = "MyApp", ClassName = "Service", MethodName = "Process" }
            }
        };

        filter.ApplyFilters(exceptionInfo);

        Assert.That(exceptionInfo.StackTrace[0].IsHighlighted, Is.True);
    }

[Test]
    public void ApplyFiltersFilteredFramesNotHighlighted()
    {
        AnalyzerConfiguration config = new()
        {
            FilteredNamespaces = new() { "System." },
            HighlightApplicationCode = true
        };
        StackTraceFilter filter = new(config);

        ExceptionInfo exceptionInfo = new()
        {
            StackTrace = new()
            {
                new() { Namespace = "System.Collections", ClassName = "List", MethodName = "Add" }
            }
        };

        filter.ApplyFilters(exceptionInfo);

        Assert.That(exceptionInfo.StackTrace[0].IsFiltered, Is.True);
        Assert.That(exceptionInfo.StackTrace[0].IsHighlighted, Is.False);
    }

[Test]
    public void ApplyFiltersCustomHighlightNamespaceHighlightsCorrectly()
    {
        AnalyzerConfiguration config = new()
        {
            HighlightedNamespaces = new() { "MyApp.Core" }
        };
        StackTraceFilter filter = new(config);

        ExceptionInfo exceptionInfo = new()
        {
            StackTrace = new()
            {
                new() { Namespace = "MyApp.Core.Services", ClassName = "DataService", MethodName = "Load" }
            }
        };

        filter.ApplyFilters(exceptionInfo);

        Assert.That(exceptionInfo.StackTrace[0].IsHighlighted, Is.True);
    }
}
