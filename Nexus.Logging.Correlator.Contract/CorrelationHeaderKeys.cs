namespace Nexus.Logging.Correlator.Contract;

/// <summary>
///     Header Keys used by the Correlator.
/// </summary>
public static class CorrelationHeaderKeys
{
    public const string ParentCorrelationId = "x-pl-parentCorrelationId";
    public const string CorrelationId = "x-pl-correlationId";
    public const string Sequence = "x-pl-sequence";

    [Obsolete(
        "Due to conflicts with native net core RequestId in Http handling this is being deprecated.  Use x-pl-stackId as a replacement.")]
    public const string RequestId = "x-pl-requestId";

    public const string StackId = "x-pl-stackId";
}