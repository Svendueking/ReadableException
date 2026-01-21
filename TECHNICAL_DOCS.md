# ReadableException - Technical Documentation

## Architecture

The ReadableException library is organized into several namespaces, each with a specific responsibility:

### Namespaces

- **ReadableException**: Core public API (`ExceptionAnalyzer`)
- **ReadableException.Models**: Data models for exceptions and stack frames
- **ReadableException.Configuration**: Configuration classes and builder
- **ReadableException.Parsing**: Exception text parsing logic
- **ReadableException.Filtering**: Stack frame filtering and highlighting

## Core Components

### 1. ExceptionAnalyzer

The main entry point for using the library. Provides two key methods:
- `Analyze(string exceptionText)` - Analyzes raw exception text
- `AnalyzeFromLog(string logEntry)` - Extracts and analyzes exceptions from log entries

### 2. ExceptionParser

Parses exception text using regular expressions:
- Exception header pattern: `(?<type>[\w\.]+(?:Exception|Error)):\s*(?<message>.*?)$`
- Stack frame pattern: `^\s*at\s+(?<method>.+?)(?:\s+in\s+(?<file>.+?):line\s+(?<line>\d+))?$`

Handles:
- Exception type and message extraction
- Stack frame parsing with file/line information
- Inner exception chains
- Nested exception hierarchies

### 3. StackTraceFilter

Applies filtering and highlighting rules based on configuration:
- Filters framework calls by namespace prefix
- Highlights application code
- Supports custom class-level filtering
- Maintains separation between filtered and visible frames

### 4. Models

#### StackTraceFrame
Represents a single stack trace frame with:
- Method name, class name, namespace
- File path and line number
- Filter and highlight flags

#### ExceptionInfo
Represents parsed exception information:
- Exception type and message
- List of stack trace frames
- Inner exception reference
- Helper methods for querying frames

#### AnalysisResult
Contains the complete analysis output:
- Root exception (innermost)
- Exception chain (all exceptions)
- Summary text
- Frame statistics
- Formatted output method

## Configuration

### Default Configuration

```csharp
{
    FilteredNamespaces = ["System.", "Microsoft.Extensions.", "Microsoft.AspNetCore."],
    HighlightedNamespaces = [],
    FilteredClassNames = [],
    HighlightedClassNames = [],
    FilterFrameworkCalls = true,
    HighlightApplicationCode = true,
    MaxStackFrames = 50,
    ShowFileInfo = true
}
```

### Configuration Builder

Fluent API for building custom configurations:

```csharp
var config = new ConfigurationBuilder()
    .FilterNamespace("System.")
    .HighlightNamespace("MyApp.")
    .WithMaxStackFrames(100)
    .Build();
```

## Design Patterns

### 1. Builder Pattern
Used in `ConfigurationBuilder` for flexible configuration creation.

### 2. Separation of Concerns
Clear separation between parsing, filtering, and analysis logic.

### 3. Immutable Results
Analysis results are populated once and not modified after creation.

## Performance Characteristics

- **Parsing**: O(n) where n is the number of lines in the exception text
- **Filtering**: O(m * k) where m is the number of frames and k is the number of filter rules
- **Memory**: Lightweight - only stores necessary frame information
- **Regex**: Pre-compiled patterns for optimal performance

## Extensibility Points

### Adding Custom Parsers

Extend `ExceptionParser` to support additional exception formats:

```csharp
public class CustomExceptionParser : ExceptionParser
{
    public override ExceptionInfo? Parse(string exceptionText)
    {
        // Custom parsing logic
    }
}
```

### Adding Custom Filters

Implement custom filtering logic by extending `StackTraceFilter`:

```csharp
public class AdvancedFilter : StackTraceFilter
{
    public AdvancedFilter(AnalyzerConfiguration config) : base(config) { }
    
    public override void ApplyFilters(ExceptionInfo exceptionInfo)
    {
        base.ApplyFilters(exceptionInfo);
        // Additional filtering logic
    }
}
```

### Custom Output Formats

Extend `AnalysisResult` for custom output formats:

```csharp
public static class AnalysisResultExtensions
{
    public static string ToJson(this AnalysisResult result)
    {
        // JSON serialization logic
    }
    
    public static string ToHtml(this AnalysisResult result)
    {
        // HTML formatting logic
    }
}
```

## Integration Patterns

### 1. ASP.NET Core Middleware

```csharp
public class ExceptionAnalysisMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ExceptionAnalyzer _analyzer;

    public ExceptionAnalysisMiddleware(RequestDelegate next)
    {
        _next = next;
        _analyzer = new ExceptionAnalyzer();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var result = _analyzer.Analyze(ex.ToString());
            // Log or return result
            throw;
        }
    }
}
```

### 2. Dependency Injection

```csharp
services.AddSingleton<ExceptionAnalyzer>(sp =>
{
    var config = new ConfigurationBuilder()
        .FilterNamespace("System.")
        .HighlightNamespace("MyApp.")
        .Build();
    return new ExceptionAnalyzer(config);
});
```

### 3. Logging Integration

```csharp
public class ExceptionLogger
{
    private readonly ILogger _logger;
    private readonly ExceptionAnalyzer _analyzer;

    public void LogException(Exception ex)
    {
        var result = _analyzer.Analyze(ex.ToString());
        _logger.LogError("Exception Analysis:\n{Analysis}", 
            result?.ToFormattedString() ?? "Failed to analyze");
    }
}
```

## Testing

The library includes comprehensive unit tests covering:
- Exception parsing with various formats
- Stack frame filtering and highlighting
- Configuration builder functionality
- Analysis result generation
- Edge cases (null inputs, incomplete data, etc.)

Run tests with:
```bash
dotnet test
```

## Building and Packaging

### Build
```bash
dotnet build -c Release
```

### Create NuGet Package
```bash
dotnet pack -c Release
```

The package is automatically created during build due to `GeneratePackageOnBuild` setting.

### Publish to NuGet
```bash
dotnet nuget push src/ReadableException/bin/Release/ReadableException.1.0.0.nupkg \
    --api-key YOUR_API_KEY \
    --source https://api.nuget.org/v3/index.json
```

## Version History

### 1.0.0 (Initial Release)
- Exception parsing and analysis
- Configurable filtering and highlighting
- NuGet package support
- Comprehensive unit tests
- Full documentation

## Future Enhancements

Potential areas for extension:
1. Support for additional exception formats (Java, Python, etc.)
2. Custom output formatters (JSON, XML, HTML)
3. Performance profiling and optimization
4. Advanced filtering rules (regex patterns, custom predicates)
5. Integration with popular logging frameworks
6. Visual Studio extension for in-IDE analysis

## Troubleshooting

### Issue: Frames not being filtered

Check that:
1. Configuration is properly set with filter rules
2. Namespace prefixes include trailing dot (e.g., "System.")
3. FilterFrameworkCalls is enabled

### Issue: No frames highlighted

Verify:
1. HighlightApplicationCode is enabled OR
2. Specific highlight rules are configured
3. Frames are not also being filtered

### Issue: Parsing fails

Ensure:
1. Exception text follows standard .NET format
2. Stack frames begin with "at "
3. File paths and line numbers match expected format

## Support and Contributions

For issues, feature requests, or contributions:
- GitHub: https://github.com/Svendueking/ReadableException
- Create an issue with detailed description
- Submit pull requests with tests
