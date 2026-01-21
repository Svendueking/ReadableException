using ReadableException;
using ReadableException.Configuration;
using System;

Console.WriteLine("=== ReadableException Real-World Examples Demo ===\n");

// Configure to filter System frames but highlight application code
AnalyzerConfiguration config = new ConfigurationBuilder()
    .FilterNamespace("System.")
    .FilterNamespace("Microsoft.")
    .HighlightNamespace("Ext.Vacation.")
    .HighlightNamespace("MaMa.")
    .WithFilterFrameworkCalls(true)
    .Build();

ExceptionAnalyzer analyzer = new ExceptionAnalyzer(config);

// Example 1: Vacation API Exception
Console.WriteLine("Example 1: Vacation TimeJob API Exception");
Console.WriteLine("=" + new string('=', 70));

string logEntry1 = @"Es gibt einen Fehler bei den folgenden Projekt: Ext.Vacation
Component: Api
Message: GetEmployee: Message=Fehler beim Laden der Daten von der TimeJob-Api. Result=Statuscode: NoContent: TimeJobApi=http://ama-ffm-web-02:41202/api/Urlaubsantrag/64078 Employeenumber=64078
Severity: Error
AdditionalData:
Exception: AF.Base.AFException: Fehler beim Laden der Daten von der TimeJob-Api. Result=Statuscode: NoContent: TimeJobApi=http://ama-ffm-web-02:41202/api/Urlaubsantrag/64078
   at Ext.Vacation.WebApi.Controllers.RequestController.GetEmployeeFromExternal(String employeenumber)
   at Ext.Vacation.WebApi.Controllers.RequestController.GetEmployee()";

ReadableException.Models.AnalysisResult? result1 = analyzer.AnalyzeFromLog(logEntry1);
if (result1 != null)
{
    Console.WriteLine(result1.ToFormattedString());
}

Console.WriteLine("\n\n");

// Example 2: FlexTime Exception with System frames
Console.WriteLine("Example 2: FlexTime Exception (with filtered System frames)");
Console.WriteLine("=" + new string('=', 70));

string logEntry2 = @"Es gibt einen Fehler bei den folgenden Projekt: Ext.Vacation
Component: Api
Message: GetFlexTimeAccount: Fehler beim Laden der AZK-Stand von der TimeJob-WebApi. Employeenumber: 63776
Severity: Error
AdditionalData:
Exception: AF.Base.AFException: Der AZK-Stand konnte nicht geladen werden.
   at Ext.Vacation.WebApi.Controllers.RequestController.GetHoursFlexTimeExternal(String employeenumber)
   at Ext.Vacation.WebApi.Controllers.RequestController.<>c__DisplayClass10_0.b__1()
   at System.Threading.ExecutionContext.RunFromThreadPoolDispatchLoop(Thread threadPoolThread, ExecutionContext executionContext, ContextCallback callback, Object state)
--- End of stack trace from previous location ---
   at System.Threading.ExecutionContext.RunFromThreadPoolDispatchLoop(Thread threadPoolThread, ExecutionContext executionContext, ContextCallback callback, Object state)
   at System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task& currentTaskSlot, Thread threadPoolThread)
--- End of stack trace from previous location ---
   at Ext.Vacation.WebApi.Controllers.RequestController.GetFlexTimeAccount()";

ReadableException.Models.AnalysisResult? result2 = analyzer.AnalyzeFromLog(logEntry2);
if (result2 != null)
{
    Console.WriteLine(result2.ToFormattedString());
}

Console.WriteLine("\n\n");

// Example 3: MaMa Document Exception
Console.WriteLine("Example 3: MaMa Document Management Exception");
Console.WriteLine("=" + new string('=', 70));

string logEntry3 = @"Timestamp:        1/1/2026 8:50:00 PM +00:00
MyProject:        MaMa.FA.ManageDocument
MyComponent:      ManageDocument 
Message:
MaMa.FA.ManageDocument / ManageDocument / WriteDocument: Beim ausführen des ApiCalls WriteDocument ist ein Fehler aufgetreten.
AdditionalData:
Statuscode: InternalServerError: Could not find a part of the path '\\amadeusag.local\tj\Files_Prod\2026-01\20260101205000Bewerbungsunterlagen.pdf'.";

// This example doesn't have a full stacktrace, but let's show what we can extract
ReadableException.Models.AnalysisResult? result3 = analyzer.AnalyzeFromLog(logEntry3);
if (result3 != null)
{
    Console.WriteLine(result3.ToFormattedString());
}
else
{
    Console.WriteLine("Note: This log entry doesn't contain a standard exception stacktrace.");
    Console.WriteLine("Message: Could not find a part of the path '\\\\amadeusag.local\\tj\\Files_Prod\\2026-01\\20260101205000Bewerbungsunterlagen.pdf'");
}

Console.WriteLine("\n\n=== Summary ===");
Console.WriteLine("The ReadableException library successfully:");
Console.WriteLine("✓ Parses AF.Base.AFException from your custom log format");
Console.WriteLine("✓ Filters out System.Threading.* framework noise");
Console.WriteLine("✓ Highlights your Ext.Vacation.* application code");
Console.WriteLine("✓ Provides clean, readable output focused on YOUR code");
Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
