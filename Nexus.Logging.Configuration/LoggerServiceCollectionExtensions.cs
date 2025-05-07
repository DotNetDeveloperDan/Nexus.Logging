using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Nexus.Logging.Configuration;

public static class LoggerServiceCollectionExtensions
{
    /// <summary>
    ///     Setup the configuration values for <see cref="ILoggerBuilder" /> from <seealso cref="IConfiguration" />.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static ILoggerBuilder ConfigureLogging(this IServiceCollection services, IConfiguration configuration)
    {
        var loggerOptions = configuration.GetSection("Logging").Get<LoggerOptions>();
        if (loggerOptions == null) loggerOptions = new LoggerOptions();

        var appScopeOptions = new ApplicationScopeOptions
        {
            ApplicationName = configuration["ApplicationName"]
                              ?? configuration["AppSettings:ApplicationName"]
                              ?? Assembly.GetCallingAssembly().GetName().Name,

            Environment = configuration["AppSettings:LoggingEnvironmentName"]
                          ?? configuration["AppSettings:Environment"]
                          ?? configuration["AppSettings:ENV"]
                          ?? configuration["ASPNETCORE_ENVIRONMENT"]
                          ?? throw new LoggerConfigurationException("Environment must be defined to configure logging.")
        };

        services.Configure<LoggerOptions>(options => configuration.GetSection("Logging").Bind(options));
        services.AddSingleton(appScopeOptions);
        services.AddSingleton(loggerOptions);
        return new LoggerBuilder(services, loggerOptions, appScopeOptions);
    }
}