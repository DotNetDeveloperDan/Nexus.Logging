using Nexus.Logging.Correlator.Contract;

namespace Nexus.Logging.Correlator.Accessors;

/// <inheritdoc cref="ICorrelationContextAccessor" />
public class CorrelationContextAccessor : ICorrelationContextAccessor
{
    private static readonly AsyncLocal<ICorrelationContext> CorrelationContext = new();

    public ICorrelationContext CurrentContext
    {
        get => CorrelationContext.Value ?? null;
        set => CorrelationContext.Value = value;
    }

    public void IncrementSequence()
    {
        CurrentContext.IncrementSequence();
    }
}