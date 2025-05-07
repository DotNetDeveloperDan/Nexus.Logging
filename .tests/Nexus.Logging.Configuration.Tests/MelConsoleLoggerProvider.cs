using System;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Nexus.Logging.Configuration.Tests;

/// <summary>
///     Test provider configuration
/// </summary>
public class MelConsoleLoggerProvider : IConfigureLoggerProvider
{
    public string ProviderName => "Microsoft.Extensions.Logging";

    public void Configure(ILoggingBuilder builder, LoggerOptions options,
        ApplicationScopeOptions applicationScopeOptions)
    {
        foreach (var filter in options.Filters)
            builder.AddFilter(filter.Key, filter.Value.GetValueOrDefault().ConvertLogLevel());

        foreach (var target in options.Targets.Where(w =>
                     w.Provider.Equals(ProviderName, StringComparison.InvariantCultureIgnoreCase)))
        {
            if (target.Type == LoggerTarget.Debug) builder.AddDebug();

            if (target.Type == LoggerTarget.Console) builder.AddConsole(opt => opt.IncludeScopes = true);
        }
    }
}