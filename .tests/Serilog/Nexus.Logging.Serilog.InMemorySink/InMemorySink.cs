using System;
using System.Collections.Generic;
using System.IO;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

namespace Nexus.Logging.Serilog.InMemorySink;

/// <summary>
///     Example Serilog Sink that is used for testing.
/// </summary>
public sealed class InMemorySink : ILogEventSink, IDisposable
{
    private static readonly Queue<string> Logs = new();

    private readonly ITextFormatter _formatter;

    public InMemorySink(ITextFormatter formatter)
    {
        _formatter = formatter;
    }

    public void Dispose()
    {
        Logs.Clear();
    }

    public void Emit(LogEvent logEvent)
    {
        using (var buffer = new StringWriter())
        {
            _formatter.Format(logEvent, buffer);
            Logs.Enqueue(buffer.ToString().Trim());
        }
    }

    public static string Pop()
    {
        return Logs.Dequeue();
    }
}