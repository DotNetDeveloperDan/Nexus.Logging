using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using System.IO;
using System.Text;

namespace Nexus.Logging.Serilog.FileSimulatorSink
{
    /// <summary>
    ///     Sink to simulate writing logs on separate lines as they would with the RollingFileSink.
    ///     <para>This is for testing purposes only, do not use this sink in a production environment.</para>
    /// </summary>
    public class FileSimulatorSink : ILogEventSink
    {
        private static readonly StringBuilder Logs = new StringBuilder();

        private readonly ITextFormatter _formatter;

        public FileSimulatorSink(ITextFormatter formatter)
        {
            _formatter = formatter;
        }

        public void Emit(LogEvent logEvent)
        {
            using (var buffer = new StringWriter())
            {
                _formatter.Format(logEvent, buffer);
                Logs.Append(buffer);
            }
        }

        public static string GetLogs()
        {
            return Logs.ToString();
        }
    }
}