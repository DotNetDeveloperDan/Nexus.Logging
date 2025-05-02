namespace Nexus.Logging.Correlator.Contract;

/// <summary>
///     Used to safely access the <see cref="ICorrelationContext" />.
/// </summary>
public interface ICorrelationContextAccessor
{
    /// <summary>
    ///     The current <see cref="ICorrelationContext" />.
    /// </summary>
    ICorrelationContext CurrentContext { get; set; }

    /// <summary>
    ///     Safely increment the SequenceId.
    /// </summary>
    void IncrementSequence();
}