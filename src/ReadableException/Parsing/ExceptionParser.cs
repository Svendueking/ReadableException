using System.Text.RegularExpressions;
using ReadableException.Models;

namespace ReadableException.Parsing;

public class ExceptionParser
{
    private static readonly Regex ExceptionHeaderPattern = new(
        @"^(?<type>[\w\.]+(?:Exception|Error)):\s*(?<message>.*?)$",
        RegexOptions.Compiled | RegexOptions.Multiline
    );

    private static readonly Regex StackFramePattern = new(
        @"^\s*at\s+(?<method>.+?)(?:\s+in\s+(?<file>.+?):line\s+(?<line>\d+))?$",
        RegexOptions.Compiled | RegexOptions.Multiline
    );

    public ExceptionInfo? Parse(string exceptionText)
    {
        if (string.IsNullOrWhiteSpace(exceptionText))
            return null;

        var lines = exceptionText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
            return null;

        var exceptionInfo = new ExceptionInfo { RawText = exceptionText };
        
        var firstLine = lines[0].Trim();
        
        if (firstLine.Contains("Inner Exception:"))
        {
            firstLine = firstLine.Substring(firstLine.IndexOf(':') + 1).Trim();
        }
        
        var match = ExceptionHeaderPattern.Match(firstLine);
        
        if (match.Success)
        {
            exceptionInfo.ExceptionType = match.Groups["type"].Value;
            exceptionInfo.Message = match.Groups["message"].Value.Trim();
        }
        else
        {
            exceptionInfo.ExceptionType = "Unknown";
            exceptionInfo.Message = firstLine;
        }

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i];
            var frameMatch = StackFramePattern.Match(line);
            
            if (frameMatch.Success)
            {
                var frame = ParseStackFrame(line, frameMatch);
                exceptionInfo.StackTrace.Add(frame);
            }
            else if (line.Trim().StartsWith("---"))
            {
                break;
            }
            else if (line.Contains("Inner Exception") || line.Contains("InnerException"))
            {
                var innerText = line.Contains(':') && line.IndexOf(':') < line.Length - 1
                    ? string.Join(Environment.NewLine, lines.Skip(i))
                    : string.Join(Environment.NewLine, lines.Skip(i + 1));
                exceptionInfo.InnerException = Parse(innerText);
                break;
            }
        }

        return exceptionInfo;
    }

    private StackTraceFrame ParseStackFrame(string fullText, Match match)
    {
        var frame = new StackTraceFrame { FullText = fullText.Trim() };
        
        var methodPart = match.Groups["method"].Value;
        ParseMethodInfo(methodPart, frame);
        
        if (match.Groups["file"].Success)
        {
            frame.FileName = match.Groups["file"].Value;
        }
        
        if (match.Groups["line"].Success && int.TryParse(match.Groups["line"].Value, out var lineNumber))
        {
            frame.LineNumber = lineNumber;
        }
        
        return frame;
    }

    private void ParseMethodInfo(string methodPart, StackTraceFrame frame)
    {
        var parenIndex = methodPart.IndexOf('(');
        if (parenIndex > 0)
        {
            methodPart = methodPart.Substring(0, parenIndex);
        }
        
        var lastDot = methodPart.LastIndexOf('.');
        if (lastDot > 0)
        {
            var fullTypeName = methodPart.Substring(0, lastDot);
            frame.MethodName = methodPart.Substring(lastDot + 1);
            
            var typeLastDot = fullTypeName.LastIndexOf('.');
            if (typeLastDot > 0)
            {
                frame.Namespace = fullTypeName.Substring(0, typeLastDot);
                frame.ClassName = fullTypeName.Substring(typeLastDot + 1);
            }
            else
            {
                frame.ClassName = fullTypeName;
            }
        }
        else
        {
            frame.MethodName = methodPart;
        }
    }
}
