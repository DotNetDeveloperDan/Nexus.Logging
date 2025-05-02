namespace Nexus.Logging.Serilog;

/// <summary>
///     Constants that define the log message properties.
/// </summary>
public static class LogProperty
{
    // Base
    public const string TimeStamp = "TimeStamp";
    public const string LogLevel = "LogLevel";
    public const string Type = "Type";
    public const string Message = "Message";
    public const string Exception = "Exception";
    public const string StackTrace = "StackTrace";
    public const string InnerExMessage = "InnerExMessage";
    public const string InnerExStackTrace = "InnerExStackTrace";
    public const string TargetSite = "TargetSite";
    public const string LogDetails = "LogDetails";

    // Scope
    public const string ApplicationName = "ApplicationName";
    public const string Environment = "Environment";
    public const string MachineName = "MachineName";
    public const string InstanceId = "InstanceId";
    public const string UserName = "UserName";
    public const string CorrelationId = "CorrelationId";
    public const string ParentCorrelationId = "ParentCorrelationId";
    public const string RequestId = "RequestId";
    public const string StackId = "StackId";
    public const string SpanId = "SpanId";
    public const string TraceId = "TraceId";
    public const string RequestPath = "RequestPath";
    public const string State = "State";
    public const string SourceContext = "SourceContext";
    public const string ParentId = "ParentId";
    public const string ActionId = "ActionId";
    public const string ActionName = "ActionName";

    // This is what the SequenceId header/scope value will be keyed
    public const string Sequence = "Sequence";

    public const string Scope = "Scope";
    public const string Event = "Event";
}