using NUnit.Framework;
using ReadableException.Parsing;
using ReadableException.Models;

namespace ReadableException.Tests;

[TestFixture]
public class RealWorldExceptionTests
{
private ExceptionAnalyzer _analyzer = null!;

    [SetUp]
    public void Setup()
    {
        _analyzer = new ExceptionAnalyzer();
    }

[Test]
    public void ParseVacationApiExceptionParsesCorrectly()
    {
        string exceptionText = @"AF.Base.AFException: Fehler beim Laden der Daten von der TimeJob-Api. Result=Statuscode: NoContent: TimeJobApi=http://ama-ffm-web-02:41202/api/Urlaubsantrag/64078
   at Ext.Vacation.WebApi.Controllers.RequestController.GetEmployeeFromExternal(String employeenumber)
   at Ext.Vacation.WebApi.Controllers.RequestController.GetEmployee()";

        ExceptionInfo? result = ExceptionParser.Parse(exceptionText);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ExceptionType, Is.EqualTo("AF.Base.AFException"));
        Assert.That(result.Message, Does.Contain("Fehler beim Laden der Daten"));
        Assert.That(result.StackTrace.Count, Is.EqualTo(2));
        
        StackTraceFrame firstFrame = result.StackTrace[0];
        Assert.That(firstFrame.Namespace, Is.EqualTo("Ext.Vacation.WebApi.Controllers"));
        Assert.That(firstFrame.ClassName, Is.EqualTo("RequestController"));
        Assert.That(firstFrame.MethodName, Is.EqualTo("GetEmployeeFromExternal"));
    }

[Test]
    public void ParseFlexTimeExceptionParsesCorrectly()
    {
        string exceptionText = @"AF.Base.AFException: Der AZK-Stand konnte nicht geladen werden.
   at Ext.Vacation.WebApi.Controllers.RequestController.GetHoursFlexTimeExternal(String employeenumber)
   at Ext.Vacation.WebApi.Controllers.RequestController.<>c__DisplayClass10_0.b__1()
   at System.Threading.ExecutionContext.RunFromThreadPoolDispatchLoop(Thread threadPoolThread, ExecutionContext executionContext, ContextCallback callback, Object state)
--- End of stack trace from previous location ---
   at System.Threading.ExecutionContext.RunFromThreadPoolDispatchLoop(Thread threadPoolThread, ExecutionContext executionContext, ContextCallback callback, Object state)
   at System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task& currentTaskSlot, Thread threadPoolThread)
--- End of stack trace from previous location ---
   at Ext.Vacation.WebApi.Controllers.RequestController.GetFlexTimeAccount()";

        ExceptionInfo? result = ExceptionParser.Parse(exceptionText);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ExceptionType, Is.EqualTo("AF.Base.AFException"));
        Assert.That(result.Message, Is.EqualTo("Der AZK-Stand konnte nicht geladen werden."));
        Assert.That(result.StackTrace.Count, Is.GreaterThan(0));
        
        // Check that we parsed frames before the separator
        StackTraceFrame firstFrame = result.StackTrace[0];
        Assert.That(firstFrame.MethodName, Is.EqualTo("GetHoursFlexTimeExternal"));
    }

[Test]
    public void AnalyzeVacationApiExceptionFiltersAndHighlightsCorrectly()
    {
        string exceptionText = @"AF.Base.AFException: Fehler beim Laden der Daten von der TimeJob-Api. Result=Statuscode: NoContent: TimeJobApi=http://ama-ffm-web-02:41202/api/Urlaubsantrag/64078
   at Ext.Vacation.WebApi.Controllers.RequestController.GetEmployeeFromExternal(String employeenumber)
   at Ext.Vacation.WebApi.Controllers.RequestController.GetEmployee()";

        AnalysisResult? result = _analyzer.Analyze(exceptionText);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.RootException, Is.Not.Null);
        Assert.That(result.RootException!.ExceptionType, Is.EqualTo("AF.Base.AFException"));
        
        // Application code should be highlighted
        List<StackTraceFrame> highlighted = result.RootException.GetHighlightedFrames();
        Assert.That(highlighted.Count, Is.GreaterThan(0));
    }

[Test]
    public void AnalyzeFlexTimeExceptionFiltersSystemFrames()
    {
        string exceptionText = @"AF.Base.AFException: Der AZK-Stand konnte nicht geladen werden.
   at Ext.Vacation.WebApi.Controllers.RequestController.GetHoursFlexTimeExternal(String employeenumber)
   at Ext.Vacation.WebApi.Controllers.RequestController.<>c__DisplayClass10_0.b__1()
   at System.Threading.ExecutionContext.RunFromThreadPoolDispatchLoop(Thread threadPoolThread, ExecutionContext executionContext, ContextCallback callback, Object state)
--- End of stack trace from previous location ---
   at System.Threading.ExecutionContext.RunFromThreadPoolDispatchLoop(Thread threadPoolThread, ExecutionContext executionContext, ContextCallback callback, Object state)
   at System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task& currentTaskSlot, Thread threadPoolThread)
--- End of stack trace from previous location ---
   at Ext.Vacation.WebApi.Controllers.RequestController.GetFlexTimeAccount()";

        AnalysisResult? result = _analyzer.Analyze(exceptionText);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.RootException, Is.Not.Null);
        
        // Should have filtered System.Threading frames
        Assert.That(result.FilteredFrames, Is.GreaterThan(0));
        
        // Application frames should be visible
        List<StackTraceFrame> visible = result.RootException!.GetVisibleFrames();
        Assert.That(visible.Count, Is.GreaterThan(0));
        
        // Check that Ext.Vacation frames are not filtered
        bool hasExtVacationFrames = visible.Any(f => f.Namespace?.StartsWith("Ext.Vacation", StringComparison.Ordinal) == true);
        Assert.That(hasExtVacationFrames, Is.True);
    }

[Test]
    public void ParseComplexLogEntryExtractsException()
    {
        string logEntry = @"Es gibt einen Fehler bei den folgenden Projekt: Ext.Vacation
Component: Api
Message: GetEmployee: Message=Fehler beim Laden der Daten von der TimeJob-Api. Result=Statuscode: NoContent: TimeJobApi=http://ama-ffm-web-02:41202/api/Urlaubsantrag/64078 Employeenumber=64078
Severity: Error
AdditionalData:
Exception: AF.Base.AFException: Fehler beim Laden der Daten von der TimeJob-Api. Result=Statuscode: NoContent: TimeJobApi=http://ama-ffm-web-02:41202/api/Urlaubsantrag/64078
   at Ext.Vacation.WebApi.Controllers.RequestController.GetEmployeeFromExternal(String employeenumber)
   at Ext.Vacation.WebApi.Controllers.RequestController.GetEmployee()";

        AnalysisResult? result = _analyzer.AnalyzeFromLog(logEntry);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.RootException, Is.Not.Null);
        Assert.That(result.RootException!.ExceptionType, Is.EqualTo("AF.Base.AFException"));
    }

[Test]
    public void ToFormattedStringVacationExceptionProducesReadableOutput()
    {
        string exceptionText = @"AF.Base.AFException: Fehler beim Laden der Daten von der TimeJob-Api. Result=Statuscode: NoContent: TimeJobApi=http://ama-ffm-web-02:41202/api/Urlaubsantrag/64078
   at Ext.Vacation.WebApi.Controllers.RequestController.GetEmployeeFromExternal(String employeenumber)
   at Ext.Vacation.WebApi.Controllers.RequestController.GetEmployee()";

        AnalysisResult? result = _analyzer.Analyze(exceptionText);

        Assert.That(result, Is.Not.Null);
        string formatted = result!.ToFormattedString();
        
        Assert.That(formatted, Does.Contain("Root Cause:"));
        Assert.That(formatted, Does.Contain("AF.Base.AFException"));
        Assert.That(formatted, Does.Contain("Key Stack Frames:"));
        Assert.That(formatted, Does.Contain("GetEmployeeFromExternal"));
    }
}
