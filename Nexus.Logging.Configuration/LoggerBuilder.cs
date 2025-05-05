using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Nexus.Logging.Configuration;

/// <inheritdoc cref="ILoggerBuilder" />
public class LoggerBuilder(
    IServiceCollection services,
    LoggerOptions options,
    ApplicationScopeOptions applicationScopeOptions)
    : ILoggerBuilder
{
    public IServiceCollection Services { get; } = services;

    public ApplicationScopeOptions ApplicationScopeOptions { get; } = applicationScopeOptions;

    public LoggerOptions LoggerOptions { get; } = options;

    public IList<IConfigureLoggerProvider> LoggerProviders { get; } = new List<IConfigureLoggerProvider>();

    public ILoggerBuilder RegisterLoggerProvider(IConfigureLoggerProvider loggerProvider)
    {
        if (!LoggerProviders.Contains(loggerProvider)) LoggerProviders.Add(loggerProvider);
        return this;
    }

    public IServiceCollection Build()
    {
        // If no IConfigureLoggerProvider registrations have been setup then find any that are referenced
        if (LoggerProviders.Count == 0)
        {
            throw new LoggerConfigurationException("No IConfigureLoggerProvider registrations have been added.");
        }
        // Setup logging through the Microsoft.Extensions.Logging pipeline
        Services.AddLogging(builder =>
        {
            builder.ClearProviders();
            // Global LogLevel
            builder.SetMinimumLevel(LoggerOptions.MinimumLevel.GetValueOrDefault().ConvertLogLevel());
            // Configure each registered provider
            foreach (var loggerProvider in LoggerProviders)
            {
                loggerProvider.Configure(builder, LoggerOptions, ApplicationScopeOptions);
            }
        });
        return Services;
    }
}