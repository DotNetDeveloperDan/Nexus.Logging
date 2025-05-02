using Nexus.Logging.Correlator.Contract;

namespace Nexus.Logging.Correlator.MessageHandlers;

/// <summary>
///     Message handler to add correlation data to outgoing request headers.
/// </summary>
public class CorrelationMessageHandler : DelegatingHandler
{
    private readonly ICorrelationContextAccessor _correlationContextAccessor;

    public CorrelationMessageHandler(ICorrelationContextAccessor correlationContext)
    {
        _correlationContextAccessor = correlationContext;
    }

    /// <summary>
    ///     Correlation message handler to increase sequence for outgoing requests
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        _correlationContextAccessor.CurrentContext.IncrementSequence();

        AddHeader(request, CorrelationHeaderKeys.CorrelationId,
            _correlationContextAccessor.CurrentContext.CorrelationId);
        AddHeader(request, CorrelationHeaderKeys.Sequence,
            _correlationContextAccessor.CurrentContext.Sequence.ToString());
        AddHeader(request, CorrelationHeaderKeys.RequestId, _correlationContextAccessor.CurrentContext.StackId);

        return base.SendAsync(request, cancellationToken);
    }

    private void AddHeader(HttpRequestMessage request, string key, string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        request.Headers.Remove(key);
        request.Headers.Add(key, value);
    }
}