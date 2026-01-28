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
    
    private static readonly char[] LineSeparators = { '\r', '\n' };

    public static ExceptionInfo? Parse(string exceptionText)
    {
        if (string.IsNullOrWhiteSpace(exceptionText))
            return null;

        string[] lines = exceptionText.Split(LineSeparators, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
            return null;

        ExceptionInfo exceptionInfo = new() { RawText = exceptionText };
        
        string firstLine = lines[0].Trim();
        
        if (firstLine.Contains("Inner Exception:"))
        {
            firstLine = firstLine.Substring(firstLine.IndexOf(':') + 1).Trim();
        }
        
        Match match = ExceptionHeaderPattern.Match(firstLine);
        
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
            string line = lines[i];
            Match frameMatch = StackFramePattern.Match(line);
            
            if (frameMatch.Success)
            {
                StackTraceFrame frame = ParseStackFrame(line, frameMatch);
                exceptionInfo.StackTrace.Add(frame);
            }
            else if (line.Trim().StartsWith("---", StringComparison.Ordinal))
            {
                break;
            }
            else if (line.Contains("Inner Exception") || line.Contains("InnerException"))
            {
                string innerText = line.Contains(':') && line.IndexOf(':') < line.Length - 1
                    ? string.Join(Environment.NewLine, lines.Skip(i))
                    : string.Join(Environment.NewLine, lines.Skip(i + 1));
                exceptionInfo.InnerException = Parse(innerText);
                break;
            }
        }

        return exceptionInfo;
    }

    private static StackTraceFrame ParseStackFrame(string fullText, Match match)
    {
        StackTraceFrame frame = new() { FullText = fullText.Trim() };
        
        string methodPart = match.Groups["method"].Value;
        ParseMethodInfo(methodPart, frame);
        
        if (match.Groups["file"].Success)
        {
            frame.FileName = match.Groups["file"].Value;
        }
        
        if (match.Groups["line"].Success && int.TryParse(match.Groups["line"].Value, out int lineNumber))
        {
            frame.LineNumber = lineNumber;
        }
        
        return frame;
    }

    private static void ParseMethodInfo(string methodPart, StackTraceFrame frame)
    {
        int parenIndex = methodPart.IndexOf('(');
        if (parenIndex > 0)
        {
            methodPart = methodPart.Substring(0, parenIndex);
        }
        
        int lastDot = methodPart.LastIndexOf('.');
        if (lastDot > 0)
        {
            string fullTypeName = methodPart.Substring(0, lastDot);
            frame.MethodName = methodPart.Substring(lastDot + 1);
            
            int typeLastDot = fullTypeName.LastIndexOf('.');
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
