using Microsoft.Extensions.DependencyInjection;

namespace Nexus.Logging.Configuration;

/// <summary>
///     Logger builder contract.
/// </summary>
public interface ILoggerBuilder
{
    /// <summary>
    ///     Reference to the <see cref="IServiceCollection" />.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    ///     Application specific configuration options.
    /// </summary>
    ApplicationScopeOptions ApplicationScopeOptions { get; }

    /// <summary>
    ///     Configuration loaded from appsettings to setup the logger targets.
    /// </summary>
    LoggerOptions LoggerOptions { get; }

    /// <summary>
    ///     A list of <see cref="IConfigureLoggerProvider" /> implementations that are registered with the builder.
    /// </summary>
    IList<IConfigureLoggerProvider> LoggerProviders { get; }

    /// <summary>
    ///     Registers an available <see cref="IConfigureLoggerProvider" /> from a logging implementation.
    /// </summary>
    /// <param name="loggerProvider"></param>
    /// <returns></returns>
    ILoggerBuilder RegisterLoggerProvider(IConfigureLoggerProvider loggerProvider);

    /// <summary>
    ///     Finalize the setup for each <see cref="IConfigureLoggerProvider" /> by executing implemented
    ///     <seealso cref="IConfigureLoggerProvider.Configure(IServiceCollection, LoggerOptions)" />.
    /// </summary>
    /// <returns></returns>
    IServiceCollection Build();
}