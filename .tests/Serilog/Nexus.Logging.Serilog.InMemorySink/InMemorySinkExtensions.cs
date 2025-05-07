using Serilog;
using Serilog.Configuration;
using Serilog.Formatting;

namespace Nexus.Logging.Serilog.InMemorySink;

public static class InMemorySinkExtensions
{
    public static LoggerConfiguration InMemory(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        ITextFormatter formatter = null)
    {
        return loggerSinkConfiguration.Sink(new InMemorySink(formatter));
    }
}