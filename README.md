# ReadableException

A .NET library for parsing and analyzing exception stacktraces to provide readable, filtered, and structured exception information. This library helps developers focus on relevant exception details by filtering out framework noise and highlighting application-specific code.

## Features

- **Parse Exception Stacktraces**: Extract and parse exception information from log entries
- **Smart Filtering**: Automatically filter out irrelevant framework calls
- **Code Highlighting**: Highlight application-specific code in stacktraces
- **Configurable**: Flexible configuration for custom filtering and highlighting rules
- **Structured Output**: Get exception information as structured objects or formatted text
- **High Performance**: Fast analysis with minimal overhead
- **Easy Integration**: Simple API that integrates seamlessly with existing logging infrastructure

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package ReadableException
```

Or via Package Manager Console:

```powershell
Install-Package ReadableException
```

## Quick Start

### Basic Usage

```csharp
using ReadableException;

var analyzer = new ExceptionAnalyzer();
var exceptionText = @"System.InvalidOperationException: Operation is not valid
   at MyApp.Service.ProcessData() in C:\Projects\MyApp\Service.cs:line 42
   at System.Threading.Tasks.Task.Execute()";

var result = analyzer.Analyze(exceptionText);

if (result != null)
{
    Console.WriteLine(result.ToFormattedString());
    Console.WriteLine($"Filtered {result.FilteredFrames} framework calls");
}
```

### Analyze from Log Entry

```csharp
var logEntry = @"2024-01-01 10:30:45 [ERROR] Application error occurred
System.InvalidOperationException: Cannot process request
   at MyApp.Controllers.HomeController.Index()
   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.Execute()";

var result = analyzer.AnalyzeFromLog(logEntry);
```

### Custom Configuration

```csharp
using ReadableException.Configuration;

var config = new ConfigurationBuilder()
    .FilterNamespace("System.")
    .FilterNamespace("Microsoft.AspNetCore.")
    .HighlightNamespace("MyApp.Core.")
    .WithMaxStackFrames(50)
    .Build();

var analyzer = new ExceptionAnalyzer(config);
var result = analyzer.Analyze(exceptionText);
```

## Configuration Options

### Default Filters

By default, the library filters these namespaces:
- `System.`
- `Microsoft.Extensions.`
- `Microsoft.AspNetCore.`

### Configuration Builder Methods

| Method | Description |
|--------|-------------|
| `FilterNamespace(string)` | Add namespace prefix to filter list |
| `HighlightNamespace(string)` | Add namespace prefix to highlight list |
| `FilterClass(string)` | Add specific class name to filter list |
| `HighlightClass(string)` | Add specific class name to highlight list |
| `WithFilterFrameworkCalls(bool)` | Enable/disable framework call filtering |
| `WithHighlightApplicationCode(bool)` | Enable/disable application code highlighting |
| `WithMaxStackFrames(int)` | Set maximum stack frames to process |
| `WithShowFileInfo(bool)` | Show/hide file information in output |

## API Reference

### ExceptionAnalyzer

Main class for analyzing exceptions.

```csharp
public class ExceptionAnalyzer
{
    public ExceptionAnalyzer();
    public ExceptionAnalyzer(AnalyzerConfiguration configuration);
    
    public AnalysisResult? Analyze(string exceptionText);
    public AnalysisResult? AnalyzeFromLog(string logEntry);
}
```

### AnalysisResult

Contains the analysis results.

```csharp
public class AnalysisResult
{
    public ExceptionInfo? RootException { get; set; }
    public List<ExceptionInfo> ExceptionChain { get; set; }
    public string Summary { get; set; }
    public int TotalFrames { get; set; }
    public int VisibleFrames { get; set; }
    public int FilteredFrames { get; set; }
    
    public string ToFormattedString();
}
```

### ExceptionInfo

Represents parsed exception information.

```csharp
public class ExceptionInfo
{
    public string ExceptionType { get; set; }
    public string Message { get; set; }
    public List<StackTraceFrame> StackTrace { get; set; }
    public ExceptionInfo? InnerException { get; set; }
    
    public List<StackTraceFrame> GetVisibleFrames();
    public List<StackTraceFrame> GetHighlightedFrames();
}
```

## Examples

### Example 1: Integration with Logging

```csharp
public class ErrorHandler
{
    private readonly ILogger<ErrorHandler> _logger;
    private readonly ExceptionAnalyzer _analyzer;

    public ErrorHandler(ILogger<ErrorHandler> logger)
    {
        _logger = logger;
        _analyzer = new ExceptionAnalyzer();
    }

    public void HandleException(Exception ex)
    {
        var exceptionText = ex.ToString();
        var result = _analyzer.Analyze(exceptionText);
        
        if (result != null)
        {
            _logger.LogError("Exception Analysis:\n{Analysis}", result.ToFormattedString());
            _logger.LogInformation("Statistics: {Visible}/{Total} frames visible", 
                result.VisibleFrames, result.TotalFrames);
        }
    }
}
```

### Example 2: Custom Application-Specific Configuration

```csharp
var config = new ConfigurationBuilder()
    .FilterNamespace("System.")
    .FilterNamespace("Microsoft.")
    .FilterNamespace("Newtonsoft.")
    .HighlightNamespace("MyCompany.MyApp.")
    .HighlightClass("DatabaseService")
    .WithFilterFrameworkCalls(true)
    .Build();

var analyzer = new ExceptionAnalyzer(config);
```

### Example 3: Processing Multiple Exceptions

```csharp
var exceptions = new List<string> 
{ 
    logEntry1, 
    logEntry2, 
    logEntry3 
};

foreach (var exceptionText in exceptions)
{
    var result = analyzer.AnalyzeFromLog(exceptionText);
    if (result?.RootException != null)
    {
        Console.WriteLine($"Root Cause: {result.RootException.ExceptionType}");
        Console.WriteLine($"Key Method: {result.RootException.GetHighlightedFrames().FirstOrDefault()?.GetFullMethodName()}");
    }
}
```

## Output Format

The formatted output includes:
- Root cause exception type and message
- Key stack frames (non-filtered)
- Highlighted frames marked with `[!]`
- File information (path and line number)
- Statistics (visible/total/filtered frame counts)

Example output:
```
Root Cause: System.InvalidOperationException: Operation is not valid

Key Stack Frames:
  at MyApp.Service.ProcessData [!]
     in C:\Projects\MyApp\Service.cs:line 42
  at MyApp.Controller.HandleRequest [!]

Statistics: 2 visible / 5 total frames (3 filtered)
```

## Performance Considerations

- The library is designed for minimal overhead
- Single stacktrace analysis typically completes in < 1ms
- Regex patterns are pre-compiled for performance
- No external dependencies beyond .NET BCL

## Requirements

- .NET 8.0 or higher
- Compatible with .NET Framework, .NET Core, and .NET 5+

## Contributing

Contributions are welcome! The library is designed to be extensible:
- Add custom parsing rules in `Parsing` namespace
- Implement custom filters in `Filtering` namespace
- Extend configuration options in `Configuration` namespace

## License

MIT License

## Support

For issues, questions, or contributions, please visit the [GitHub repository](https://github.com/Svendueking/ReadableException). 
