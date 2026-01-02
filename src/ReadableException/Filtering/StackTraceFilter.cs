using ReadableException.Configuration;
using ReadableException.Models;

namespace ReadableException.Filtering;

public class StackTraceFilter
{
    private readonly AnalyzerConfiguration _configuration;

    public StackTraceFilter(AnalyzerConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ApplyFilters(ExceptionInfo exceptionInfo)
    {
        if (exceptionInfo == null)
            return;

        foreach (var frame in exceptionInfo.StackTrace)
        {
            ApplyFilterRules(frame);
            ApplyHighlightRules(frame);
        }

        if (exceptionInfo.InnerException != null)
        {
            ApplyFilters(exceptionInfo.InnerException);
        }
    }

    private void ApplyFilterRules(StackTraceFrame frame)
    {
        if (string.IsNullOrEmpty(frame.Namespace))
            return;

        if (_configuration.FilterFrameworkCalls)
        {
            foreach (var filterPrefix in _configuration.FilteredNamespaces)
            {
                if (frame.Namespace.StartsWith(filterPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    frame.IsFiltered = true;
                    return;
                }
            }
        }

        foreach (var filterClass in _configuration.FilteredClassNames)
        {
            if (string.Equals(frame.ClassName, filterClass, StringComparison.OrdinalIgnoreCase))
            {
                frame.IsFiltered = true;
                return;
            }
        }
    }

    private void ApplyHighlightRules(StackTraceFrame frame)
    {
        if (frame.IsFiltered)
            return;

        if (string.IsNullOrEmpty(frame.Namespace))
            return;

        foreach (var highlightPrefix in _configuration.HighlightedNamespaces)
        {
            if (frame.Namespace.StartsWith(highlightPrefix, StringComparison.OrdinalIgnoreCase))
            {
                frame.IsHighlighted = true;
                return;
            }
        }

        foreach (var highlightClass in _configuration.HighlightedClassNames)
        {
            if (string.Equals(frame.ClassName, highlightClass, StringComparison.OrdinalIgnoreCase))
            {
                frame.IsHighlighted = true;
                return;
            }
        }

        if (_configuration.HighlightApplicationCode && !frame.IsFiltered)
        {
            var isFrameworkCode = _configuration.FilteredNamespaces.Any(prefix =>
                frame.Namespace.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
            
            if (!isFrameworkCode)
            {
                frame.IsHighlighted = true;
            }
        }
    }
}
