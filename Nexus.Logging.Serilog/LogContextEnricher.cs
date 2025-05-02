using Nexus.Logging.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Nexus.Logging.Serilog;

/// <inheritdoc />
public class LogContextEnricher : ILogEventEnricher
{
    private readonly ApplicationScopeOptions _asOptions;

    // ApplicationScopeOptions supplied through constructor injection
    public LogContextEnricher(ApplicationScopeOptions asOptions)
    {
        _asOptions = asOptions;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory factory)
    {
        logEvent.AddPropertyIfAbsent(factory.CreateProperty(LogProperty.ApplicationName, _asOptions.ApplicationName));
        logEvent.AddPropertyIfAbsent(factory.CreateProperty(LogProperty.Environment, _asOptions.Environment));
        logEvent.AddPropertyIfAbsent(factory.CreateProperty(LogProperty.InstanceId, _asOptions.InstanceId));
        logEvent.AddPropertyIfAbsent(factory.CreateProperty(LogProperty.MachineName, _asOptions.MachineName));
        logEvent.AddPropertyIfAbsent(factory.CreateProperty(LogProperty.UserName, _asOptions.UserName));
    }
}