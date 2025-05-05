using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace Nexus.Logging.Tests
{
    public sealed class InMemoryLogProvider : ILoggerProvider, ISupportExternalScope
    {
        private readonly ConcurrentDictionary<string, InMemoryLogger> _loggers = new ConcurrentDictionary<string, InMemoryLogger>();

        private IExternalScopeProvider _scopeProvider;

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, CreateLoggerImplementation);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        private InMemoryLogger CreateLoggerImplementation(string categoryName)
        {
            return new InMemoryLogger() { ScopeProvider = _scopeProvider };
        }

        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;

            foreach (var logger in _loggers)
            {
                logger.Value.ScopeProvider = _scopeProvider;
            }
        }
    }
}
