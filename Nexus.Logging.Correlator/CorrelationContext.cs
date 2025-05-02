using Nexus.Logging.Correlator.Contract;

namespace Nexus.Logging.Correlator;

/// <summary>
///     Correlation context implementation
/// </summary>
public class CorrelationContext : ICorrelationContext
{
    private int _sequence;

    public CorrelationContext(
        string correlationId,
        string parentCorrelationId,
        string stackId,
        int sequence = 0)
    {
        CorrelationId = correlationId;
        ParentCorrelationId = parentCorrelationId;
        StackId = stackId;
        _sequence = sequence;
    }

    public string CorrelationId { get; }
    public int Sequence => _sequence;
    public string ParentCorrelationId { get; }
    public string StackId { get; }

    /// <summary>
    ///     Increments sequence number by one
    /// </summary>
    public void IncrementSequence()
    {
        Interlocked.Increment(ref _sequence);
    }
}