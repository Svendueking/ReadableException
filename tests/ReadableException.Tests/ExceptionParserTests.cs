using ReadableException.Parsing;
using Xunit;

namespace ReadableException.Tests;

public class ExceptionParserTests
{
    private readonly ExceptionParser _parser;

    public ExceptionParserTests()
    {
        _parser = new ExceptionParser();
    }

    [Fact]
    public void Parse_ValidException_ReturnsExceptionInfo()
    {
        var exceptionText = @"System.InvalidOperationException: Operation is not valid
   at MyApp.Service.ProcessData() in C:\Projects\MyApp\Service.cs:line 42
   at MyApp.Controller.HandleRequest()";

        var result = _parser.Parse(exceptionText);

        Assert.NotNull(result);
        Assert.Equal("System.InvalidOperationException", result.ExceptionType);
        Assert.Equal("Operation is not valid", result.Message);
        Assert.Equal(2, result.StackTrace.Count);
    }

    [Fact]
    public void Parse_EmptyString_ReturnsNull()
    {
        var result = _parser.Parse("");

        Assert.Null(result);
    }

    [Fact]
    public void Parse_NullString_ReturnsNull()
    {
        var result = _parser.Parse(null!);

        Assert.Null(result);
    }

    [Fact]
    public void Parse_StackFrameWithFileInfo_ParsesCorrectly()
    {
        var exceptionText = @"System.Exception: Test
   at MyApp.Service.Method() in C:\Path\File.cs:line 123";

        var result = _parser.Parse(exceptionText);

        Assert.NotNull(result);
        Assert.Single(result.StackTrace);
        var frame = result.StackTrace[0];
        Assert.Equal("Method", frame.MethodName);
        Assert.Equal("Service", frame.ClassName);
        Assert.Equal("MyApp", frame.Namespace);
        Assert.Equal(@"C:\Path\File.cs", frame.FileName);
        Assert.Equal(123, frame.LineNumber);
    }

    [Fact]
    public void Parse_StackFrameWithoutFileInfo_ParsesCorrectly()
    {
        var exceptionText = @"System.Exception: Test
   at System.Collections.Generic.List`1.Add(T item)";

        var result = _parser.Parse(exceptionText);

        Assert.NotNull(result);
        Assert.Single(result.StackTrace);
        var frame = result.StackTrace[0];
        Assert.Null(frame.FileName);
        Assert.Null(frame.LineNumber);
    }

    [Fact]
    public void Parse_WithInnerException_ParsesInnerException()
    {
        var exceptionText = @"System.InvalidOperationException: Outer exception
   at OuterMethod()
Inner Exception: System.ArgumentException: Inner exception
   at InnerMethod()";

        var result = _parser.Parse(exceptionText);

        Assert.NotNull(result);
        Assert.Equal("System.InvalidOperationException", result.ExceptionType);
        Assert.NotNull(result.InnerException);
        Assert.Equal("System.ArgumentException", result.InnerException.ExceptionType);
    }
}
