using Nexus.Logging.Correlator.Contract;

namespace Nexus.Logging.Correlator;

public sealed class CorrelationContextFactory : ICorrelationContextFactory
{
    private readonly ICorrelationContextAccessor _correlationContextAccessor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CorrelationContextFactory" /> class.
    /// </summary>
    /// <param name="correlationContextAccessor">
    ///     The <see cref="ICorrelationContextAccessor" /> through which the
    ///     <see cref="ICorrelationContext" /> will be set.
    /// </param>
    public CorrelationContextFactory(ICorrelationContextAccessor correlationContextAccessor)
    {
        _correlationContextAccessor = correlationContextAccessor;
    }

    /// <inheritdoc />
    public ICorrelationContext Create(string correlationId, string parentCorrelationId, string requestId,
        int sequence = 0)
    {
        var correlationContext = new CorrelationContext(correlationId, parentCorrelationId, requestId, sequence);

        if (_correlationContextAccessor != null) _correlationContextAccessor.CurrentContext = correlationContext;

        return correlationContext;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_correlationContextAccessor != null) _correlationContextAccessor.CurrentContext = null;
    }
}