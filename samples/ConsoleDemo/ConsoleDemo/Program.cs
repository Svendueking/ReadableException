using ReadableException;
using ReadableException.Configuration;

Console.WriteLine("=== ReadableException Library Demo ===\n");

string exampleException = @"System.InvalidOperationException: The operation cannot be completed at this time
   at MyApp.Services.DataService.LoadData() in C:\Projects\MyApp\Services\DataService.cs:line 45
   at MyApp.Services.DataService.ProcessRequest(String requestId)
   at System.Threading.Tasks.Task.InnerInvoke()
   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)
   at System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task& currentTaskSlot, Thread threadPoolThread)
   at MyApp.Controllers.ApiController.HandleRequest() in C:\Projects\MyApp\Controllers\ApiController.cs:line 78
   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.Execute(Object controller, Object[] parameters)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeActionMethodAsync()";

Console.WriteLine("Example 1: Basic Analysis with Default Configuration");
Console.WriteLine("=" + new string('=', 55));

ExceptionAnalyzer analyzer = new ExceptionAnalyzer();
ReadableException.Models.AnalysisResult? result = analyzer.Analyze(exampleException);

if (result != null)
{
    Console.WriteLine(result.ToFormattedString());
}

Console.WriteLine("\n\nExample 2: Custom Configuration - Highlight MyApp namespace");
Console.WriteLine("=" + new string('=', 55));

AnalyzerConfiguration customConfig = new ConfigurationBuilder()
    .FilterNamespace("System.")
    .FilterNamespace("Microsoft.")
    .HighlightNamespace("MyApp.")
    .WithFilterFrameworkCalls(true)
    .Build();

ExceptionAnalyzer customAnalyzer = new ExceptionAnalyzer(customConfig);
ReadableException.Models.AnalysisResult? customResult = customAnalyzer.Analyze(exampleException);

if (customResult != null)
{
    Console.WriteLine(customResult.ToFormattedString());
}

Console.WriteLine("\n\nExample 3: Analyzing from Log Entry");
Console.WriteLine("=" + new string('=', 55));

string logEntry = @"2024-01-02 15:30:45 [ERROR] Unhandled exception in request processing
System.ArgumentNullException: Value cannot be null. (Parameter 'userId')
   at MyApp.Services.UserService.GetUser(String userId) in C:\Projects\MyApp\Services\UserService.cs:line 23
   at MyApp.Controllers.UserController.GetUserProfile()
   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.Execute()";

ReadableException.Models.AnalysisResult? logResult = analyzer.AnalyzeFromLog(logEntry);

if (logResult != null)
{
    Console.WriteLine($"Summary: {logResult.Summary}");
    Console.WriteLine($"Statistics: {logResult.VisibleFrames} visible / {logResult.TotalFrames} total / {logResult.FilteredFrames} filtered");
    
    Console.WriteLine("\nHighlighted Frames (Application Code):");
    foreach (ReadableException.Models.StackTraceFrame frame in logResult.RootException?.GetHighlightedFrames() ?? new List<ReadableException.Models.StackTraceFrame>())
    {
        Console.WriteLine($"  - {frame.GetFullMethodName()}");
        if (!string.IsNullOrEmpty(frame.FileName))
        {
            Console.WriteLine($"    {frame.FileName}:line {frame.LineNumber}");
        }
    }
}

Console.WriteLine("\n\nPress any key to exit...");
Console.ReadKey();

