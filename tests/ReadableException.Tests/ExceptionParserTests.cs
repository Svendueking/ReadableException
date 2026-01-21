using NUnit.Framework;
using ReadableException.Parsing;

namespace ReadableException.Tests;

[TestFixture]
public class ExceptionParserTests
{
    private ExceptionParser _parser = null!;

    [SetUp]
    public void Setup()
    {
        _parser = new ExceptionParser();
    }

    [Test]
    public void Parse_ValidException_ReturnsExceptionInfo()
    {
        string exceptionText = @"System.InvalidOperationException: Operation is not valid
   at MyApp.Service.ProcessData() in C:\Projects\MyApp\Service.cs:line 42
   at MyApp.Controller.HandleRequest()";

        ReadableException.Models.ExceptionInfo? result = _parser.Parse(exceptionText);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ExceptionType, Is.EqualTo("System.InvalidOperationException"));
        Assert.That(result.Message, Is.EqualTo("Operation is not valid"));
        Assert.That(result.StackTrace.Count, Is.EqualTo(2));
    }

    [Test]
    public void Parse_EmptyString_ReturnsNull()
    {
        ReadableException.Models.ExceptionInfo? result = _parser.Parse("");

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Parse_NullString_ReturnsNull()
    {
        ReadableException.Models.ExceptionInfo? result = _parser.Parse(null!);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Parse_StackFrameWithFileInfo_ParsesCorrectly()
    {
        string exceptionText = @"System.Exception: Test
   at MyApp.Service.Method() in C:\Path\File.cs:line 123";

        ReadableException.Models.ExceptionInfo? result = _parser.Parse(exceptionText);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StackTrace.Count, Is.EqualTo(1));
        ReadableException.Models.StackTraceFrame frame = result.StackTrace[0];
        Assert.That(frame.MethodName, Is.EqualTo("Method"));
        Assert.That(frame.ClassName, Is.EqualTo("Service"));
        Assert.That(frame.Namespace, Is.EqualTo("MyApp"));
        Assert.That(frame.FileName, Is.EqualTo(@"C:\Path\File.cs"));
        Assert.That(frame.LineNumber, Is.EqualTo(123));
    }

    [Test]
    public void Parse_StackFrameWithoutFileInfo_ParsesCorrectly()
    {
        string exceptionText = @"System.Exception: Test
   at System.Collections.Generic.List`1.Add(T item)";

        ReadableException.Models.ExceptionInfo? result = _parser.Parse(exceptionText);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StackTrace.Count, Is.EqualTo(1));
        ReadableException.Models.StackTraceFrame frame = result.StackTrace[0];
        Assert.That(frame.FileName, Is.Null);
        Assert.That(frame.LineNumber, Is.Null);
    }

    [Test]
    public void Parse_WithInnerException_ParsesInnerException()
    {
        string exceptionText = @"System.InvalidOperationException: Outer exception
   at OuterMethod()
Inner Exception: System.ArgumentException: Inner exception
   at InnerMethod()";

        ReadableException.Models.ExceptionInfo? result = _parser.Parse(exceptionText);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ExceptionType, Is.EqualTo("System.InvalidOperationException"));
        Assert.That(result.InnerException, Is.Not.Null);
        Assert.That(result.InnerException!.ExceptionType, Is.EqualTo("System.ArgumentException"));
    }
}
