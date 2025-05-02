using Microsoft.AspNetCore.Http;
using Nexus.Logging.Contract;
using Nexus.Logging.Correlator.Contract;
using Nexus.Logging.Correlator.Extensions;

namespace Nexus.Logging.Correlator;

/// <summary>
///     Middleware that pulls the CorrelationId from the configured HTTP Header and adds it to the correlation context.
///     Also makes the <see cref="CorrelationContext.CorrelationId" /> available for the Async pipeline.
///     If no HTTP Header is in the Request it will generate a new CorrelationId and return it to the caller in the HTTP
///     Header of the Response.
/// </summary>
public class CorrelationMiddleware
{
    private readonly ILogger<CorrelationMiddleware> _logger;
    private readonly RequestDelegate _next;

    public CorrelationMiddleware(ILogger<CorrelationMiddleware> logger, RequestDelegate next)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task InvokeAsync(HttpContext context, ICorrelationContextFactory correlationContextFactory)
    {
        context.Request.Headers.TryGetValue(CorrelationHeaderKeys.CorrelationId, out var correlationId);
        var parentCorrelationId = correlationId;
        correlationId = Guid.NewGuid().ToString();
        context.Request.Headers[CorrelationHeaderKeys.CorrelationId] = correlationId;

        context.Request.Headers.TryGetValue(CorrelationHeaderKeys.Sequence, out var sequenceValue);
        int.TryParse(sequenceValue, out var sequence);

        if (!context.Request.Headers.TryGetValue(CorrelationHeaderKeys.StackId, out var stackId)
            && !context.Request.Headers.TryGetValue(CorrelationHeaderKeys.RequestId, out stackId))
        {
            stackId = Guid.NewGuid().ToString();
            context.Request.Headers[CorrelationHeaderKeys.StackId] = stackId;
        }

        var correlationContext =
            correlationContextFactory.Create(correlationId, parentCorrelationId, stackId, sequence);
        using (_logger.BeginScope(correlationContext.ToDictionary()))
        {
            _logger.Log(LogLevel.Info, "Correlator initialized for the request...");

            context.Response.OnStarting(() =>
            {
                AddResponseHeader(context, CorrelationHeaderKeys.CorrelationId, correlationContext.CorrelationId);
                AddResponseHeader(context, CorrelationHeaderKeys.ParentCorrelationId,
                    correlationContext.ParentCorrelationId);
                AddResponseHeader(context, CorrelationHeaderKeys.Sequence, correlationContext.Sequence.ToString());
                AddResponseHeader(context, CorrelationHeaderKeys.StackId, correlationContext.StackId);

                return Task.CompletedTask;
            });

            await _next(context);

            correlationContextFactory.Dispose();
        }
    }

    private void AddResponseHeader(HttpContext context, string headerKey, string headerValue)
    {
        if (!string.IsNullOrWhiteSpace(headerKey) && !string.IsNullOrWhiteSpace(headerValue)
                                                  && context?.Response?.Headers != null)
            context.Response.Headers[headerKey] = headerValue;
    }
}