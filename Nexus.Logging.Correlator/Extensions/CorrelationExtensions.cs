using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nexus.Logging.Correlator.Accessors;
using Nexus.Logging.Correlator.Contract;
using Nexus.Logging.Correlator.MessageHandlers;

namespace Nexus.Logging.Correlator.Extensions;

public static class CorrelationExtensions
{
    /// <summary>
    ///     Adds required services to support the Correlation ID functionality.
    /// </summary>
    /// <param name="serviceCollection"></param>
    public static IServiceCollection AddRequestCorrelation(this IServiceCollection serviceCollection)
    {
        serviceCollection.TryAddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
        serviceCollection.TryAddTransient<CorrelationMessageHandler>();
        serviceCollection.TryAddTransient<ICorrelationContextFactory, CorrelationContextFactory>();

        return serviceCollection;
    }

    /// <summary>
    ///     Enable correlation context logging.
    /// </summary>
    /// <param name="app"></param>
    public static IApplicationBuilder UseRequestCorrelation(this IApplicationBuilder app)
    {
        if (app.ApplicationServices.GetService(typeof(ICorrelationContextFactory)) == null)
            throw new InvalidOperationException("Must register services by using AddRequestCorrelation()");

        return app.UseMiddleware<CorrelationMiddleware>();
    }

    /// <summary>
    ///     Add message handler for correlation data to IHttpClientFactory
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IHttpClientBuilder AddCorrelationHandler(this IHttpClientBuilder builder)
    {
        return builder.AddHttpMessageHandler<CorrelationMessageHandler>();
    }
}