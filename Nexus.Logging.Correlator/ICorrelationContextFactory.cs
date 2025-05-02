using Nexus.Logging.Correlator.Contract;

namespace Nexus.Logging.Correlator
{
    public interface ICorrelationContextFactory : IDisposable
    {
        /// <summary>
        /// Creates correlation context
        /// </summary>
        /// <param name="correlationId">Identifier to tie logs together within the active callstack boundary.</param>
        /// <param name="parentCorrelationId">CorrelationId from caller. Will be empty if it is the first call in the chain.</param>
        /// <param name="requestId">Trace identifier that remains constant across boundaries of a request.</param>
        /// <param name="sequence">Incrementing value to represent ordering of boundary calls.  Will be deprecated in the near future.</param>
        /// <returns><see cref="ICorrelationContext"/></returns>
        ICorrelationContext Create(string correlationId, string parentCorrelationId, string requestId, int sequence = 0);
    }
}