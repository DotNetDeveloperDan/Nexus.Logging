using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Nexus.Logging.Configuration;

namespace Nexus.Logging.Tests;

public class ConfigureInMemoryProvider : IConfigureLoggerProvider
{
    public string ProviderName => "InMemory";

    public void Configure(ILoggingBuilder builder, LoggerOptions loggerOptions, ApplicationScopeOptions scopeOptions)
    {
        if (loggerOptions.Targets.Any(a =>
                a.Provider.Equals(ProviderName, StringComparison.InvariantCultureIgnoreCase)))
            builder.AddProvider(new InMemoryLogProvider());
    }
}