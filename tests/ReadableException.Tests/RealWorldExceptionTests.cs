using NUnit.Framework;
using ReadableException.Parsing;
using ReadableException.Models;

namespace ReadableException.Tests;

[TestFixture]
public class RealWorldExceptionTests
{
    private ExceptionParser _parser = null!;
    private ExceptionAnalyzer _analyzer = null!;

    [SetUp]
    public void Setup()
    {
        _parser = new ExceptionParser();
        _analyzer = new ExceptionAnalyzer();
    }

    [Test]
    public void Parse_VacationApiException_ParsesCorrectly()
    {
        string exceptionText = @"AF.Base.AFException: Fehler beim Laden der Daten von der TimeJob-Api. Result=Statuscode: NoContent: TimeJobApi=http://ama-ffm-web-02:41202/api/Urlaubsantrag/64078
   at Ext.Vacation.WebApi.Controllers.RequestController.GetEmployeeFromExternal(String employeenumber)
   at Ext.Vacation.WebApi.Controllers.RequestController.GetEmployee()";

        ExceptionInfo? result = _parser.Parse(exceptionText);

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
    public void Parse_FlexTimeException_ParsesCorrectly()
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

        ExceptionInfo? result = _parser.Parse(exceptionText);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ExceptionType, Is.EqualTo("AF.Base.AFException"));
        Assert.That(result.Message, Is.EqualTo("Der AZK-Stand konnte nicht geladen werden."));
        Assert.That(result.StackTrace.Count, Is.GreaterThan(0));
        
        // Check that we parsed frames before the separator
        StackTraceFrame firstFrame = result.StackTrace[0];
        Assert.That(firstFrame.MethodName, Is.EqualTo("GetHoursFlexTimeExternal"));
    }

    [Test]
    public void Analyze_VacationApiException_FiltersAndHighlightsCorrectly()
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
    public void Analyze_FlexTimeException_FiltersSystemFrames()
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
        bool hasExtVacationFrames = visible.Any(f => f.Namespace?.StartsWith("Ext.Vacation") == true);
        Assert.That(hasExtVacationFrames, Is.True);
    }

    [Test]
    public void Parse_ComplexLogEntry_ExtractsException()
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
    public void ToFormattedString_VacationException_ProducesReadableOutput()
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
