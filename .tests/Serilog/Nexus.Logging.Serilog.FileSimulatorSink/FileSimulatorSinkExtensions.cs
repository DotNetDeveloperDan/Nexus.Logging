using Serilog;
using Serilog.Configuration;
using Serilog.Formatting;

namespace Nexus.Logging.Serilog.FileSimulatorSink;

public static class FileSimulatorSinkExtensions
{
    public static LoggerConfiguration FileSimulator(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        ITextFormatter formatter = null)
    {
        return loggerSinkConfiguration.Sink(new FileSimulatorSink(formatter));
    }
}