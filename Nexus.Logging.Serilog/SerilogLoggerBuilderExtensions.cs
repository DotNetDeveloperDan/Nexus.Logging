using Nexus.Logging.Configuration;

namespace Nexus.Logging.Serilog;

public static class SerilogLoggerBuilderExtensions
{
    /// <summary>
    ///     Register Serilog as a Provider based on the Targets processed by <see cref="IConfigureLoggerProvider" />.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static ILoggerBuilder RegisterSerilog(this ILoggerBuilder builder)
    {
        return builder.RegisterLoggerProvider(new SerilogConfigureLoggerProvider());
    }
}