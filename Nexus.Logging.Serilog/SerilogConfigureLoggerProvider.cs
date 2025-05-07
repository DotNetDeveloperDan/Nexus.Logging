using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Nexus.Logging.Configuration;
using Serilog;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using LogLevel = Nexus.Logging.Contract.LogLevel;

namespace Nexus.Logging.Serilog;

/// <summary>
///     <see cref="IConfigureLoggerProvider" /> implementation to register Serilog Targets.
/// </summary>
public class SerilogConfigureLoggerProvider : IConfigureLoggerProvider
{
    private const string NexusJsonLogFormatter = "$NexusJsonLogFormatter";
    private string _applicationName;
    private string _environment;
    private LogLevel _minimumLogLevel;
    public string ProviderName => "Serilog";

    /// <summary>
    ///     Configure Serilog for Targets.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="loggerOptions">Configuration options</param>
    public void Configure(ILoggingBuilder builder, LoggerOptions loggerOptions,
        ApplicationScopeOptions applicationScopeOptions)
    {
        _applicationName = applicationScopeOptions.ApplicationName;
        _environment = applicationScopeOptions.Environment;
        _minimumLogLevel = loggerOptions.MinimumLevel.GetValueOrDefault();
        if (loggerOptions.Targets.Any(a =>
                a.Provider.Equals(ProviderName, StringComparison.InvariantCultureIgnoreCase)))
        {
            var loggerConfig = new LoggerConfiguration();
            loggerConfig.MinimumLevel.Is(loggerOptions.MinimumLevel.GetValueOrDefault().ToSerilogLogLevel());

            foreach (var filter in loggerOptions.Filters)
                loggerConfig.MinimumLevel.Override(filter.Key, filter.Value.GetValueOrDefault().ToSerilogLogLevel());

            var defaultOutputFormatter = GetOutputFormatter(loggerOptions.OutputTemplate);

            foreach (var target in loggerOptions.Targets) ConfigureTarget(loggerConfig, target, defaultOutputFormatter);

            //for enriching syslog content, captures all log events and adds content in the LogContentEnricher if it does not already exist
            loggerConfig.Enrich.With(new LogContextEnricher(applicationScopeOptions));

            // Should dispose logger when the Provider is disposed by the framework
            builder.AddSerilog(loggerConfig.CreateLogger(), true);
        }
    }

    /// <summary>
    ///     Determine which <see cref="ITextFormatter" /> should be used based on the OutputTemplate configuration option.
    ///     <para>Prefix with a '$' to refer to a custom <see cref="ITextFormatter" /> implementation.</para>
    ///     <para>Lack of '$' prefix will indicate that a tokenized template should be used.</para>
    /// </summary>
    /// <param name="outputTemplate"></param>
    /// <returns></returns>
    public ITextFormatter GetOutputFormatter(string outputTemplate)
    {
        if (string.IsNullOrWhiteSpace(outputTemplate)) outputTemplate = NexusJsonLogFormatter;

        // Determine if a custom formatter should be used
        if (outputTemplate.IndexOf('$') == 0)
            if (outputTemplate.Equals(NexusJsonLogFormatter, StringComparison.OrdinalIgnoreCase))
                return new NexusJsonLogFormatter();

        // Default
        return new MessageTemplateTextFormatter(outputTemplate);
    }

    /// <summary>
    ///     Configure a <see cref="LoggerTarget" /> based on configured the <seealso cref="LoggerTargetOptions.Type" />.
    /// </summary>
    /// <param name="loggerConfig">Configuration to add the target to.</param>
    /// <param name="targetOptions"><see cref="LoggerTargetOptions" /> to use to configure the target.</param>
    /// <param name="formatter"><see cref="ITextFormatter" /> configured in <seealso cref="LoggerOptions.OutputTemplate" />.</param>
    internal void ConfigureTarget(LoggerConfiguration loggerConfig, LoggerTargetOptions targetOptions,
        ITextFormatter formatter)
    {
        var localFormatter = formatter;
        // Check to see if the Target has an OutputTemplate value specified that should override the master value
        if (!string.IsNullOrWhiteSpace(targetOptions.OutputTemplate))
            localFormatter = GetOutputFormatter(targetOptions.OutputTemplate);

        switch (targetOptions.Type)
        {
            case LoggerTarget.Debug:
                ConfigureDebug(loggerConfig, targetOptions, localFormatter);
                break;

            case LoggerTarget.Console:
                ConfigureConsole(loggerConfig, targetOptions, localFormatter);
                break;

            case LoggerTarget.RollingFile:
                ConfigureRollingFile(loggerConfig, targetOptions, localFormatter);
                break;

            case LoggerTarget.Service:
                ConfigureService(loggerConfig, targetOptions, localFormatter);
                break;
        }
    }

    /// <summary>
    ///     Configure a <see cref="LoggerTarget.Console" /> target.
    /// </summary>
    /// <param name="loggerConfig"></param>
    /// <param name="targetOptions"></param>
    internal void ConfigureConsole(LoggerConfiguration loggerConfig, LoggerTargetOptions targetOptions,
        ITextFormatter formatter)
    {
        loggerConfig.WriteTo.Console(
            formatter,
            targetOptions.Args.GetValueOrDefault("MinimumLevel", _minimumLogLevel).ToSerilogLogLevel());
    }

    /// <summary>
    ///     Configure a <see cref="LoggerTarget.Debug" /> target.
    /// </summary>
    /// <param name="loggerConfig"></param>
    /// <param name="targetOptions"></param>
    internal void ConfigureDebug(LoggerConfiguration loggerConfig, LoggerTargetOptions targetOptions,
        ITextFormatter formatter)
    {
        loggerConfig.WriteTo.Debug(
            formatter,
            targetOptions.Args.GetValueOrDefault("MinimumLevel", _minimumLogLevel).ToSerilogLogLevel());
    }

    /// <summary>
    ///     Configure a <see cref="LoggerTarget.RollingFile" /> target.
    /// </summary>
    /// <param name="loggerConfig"></param>
    /// <param name="targetOptions"></param>
    internal void ConfigureRollingFile(LoggerConfiguration loggerConfig, LoggerTargetOptions targetOptions,
        ITextFormatter formatter)
    {
        var logFile = ReplaceLogPathFormatTokens(targetOptions.Args.GetValueOrDefault<string>("PathFormat", null));
        if (string.IsNullOrWhiteSpace(logFile))
        {
            // If the PathFormat was not specified error out as it is a configuration issue
            var errorMessage = "Unable to initialize RollingFile logger without a 'PathFormat' target arg.";
            Console.WriteLine(errorMessage);
            throw new LoggerConfigurationException(errorMessage);
        }

        if (!IsFileSystemAccessible(logFile))
        {
            var tempLogFile = ReplaceLogPathFormatTokens(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "{Environment}", "{ApplicationName}.log"));
            if (!IsFileSystemAccessible(tempLogFile))
            {
                // If the tempLogFile can't be accessed then something is seriously wrong with permissions and unexpected behavior could result
                var errorMessage =
                    $"Unable to access file system for log file: '{logFile}' or temporary log file: '{tempLogFile}'.";
                Console.WriteLine(errorMessage);
                throw new LoggerConfigurationException(errorMessage);
            }

            Console.WriteLine("Unable to access log file path format: '{0}'. Using temporary path format: '{1}'",
                logFile, tempLogFile);
            logFile = tempLogFile;
        }

        if (!Path.GetFileNameWithoutExtension(logFile).EndsWith("-"))
            logFile = Path.Combine(Path.GetDirectoryName(logFile),
                string.Concat(Path.GetFileNameWithoutExtension(logFile), "-", Path.GetExtension(logFile)));

        loggerConfig.WriteTo.File(formatter,
            logFile,
            targetOptions.Args.GetValueOrDefault("MinimumLevel", _minimumLogLevel).ToSerilogLogLevel(),
            targetOptions.Args.GetValueOrDefault("FileSize", (long)10485760),
            retainedFileCountLimit: targetOptions.Args.GetValueOrDefault("FileCount", 10),
            buffered: targetOptions.Args.GetValueOrDefault("Buffered", false),
            shared: targetOptions.Args.GetValueOrDefault("Shared", true),
            rollingInterval: RollingInterval.Day,
            rollOnFileSizeLimit: true);
    }

    /// <summary>
    ///     Configure a <see cref="LoggerTarget.Service" /> target.
    /// </summary>
    /// <param name="loggerConfig"></param>
    /// <param name="targetOptions"></param>
    /// <param name="formatter"></param>
    internal void ConfigureService(LoggerConfiguration loggerConfig, LoggerTargetOptions targetOptions,
        ITextFormatter formatter)
    {
        ServiceTargetSinkConfiguration.Register(targetOptions, formatter, loggerConfig.WriteTo);
    }

    /// <summary>
    ///     Replace designated tokens in the specified PathFormat argument.
    /// </summary>
    /// <param name="pathFormat"></param>
    /// <returns></returns>
    private string ReplaceLogPathFormatTokens(string pathFormat)
    {
        return string.IsNullOrWhiteSpace(pathFormat)
            ? pathFormat
            : pathFormat.Replace("{ApplicationName}", _applicationName.Replace(".", string.Empty))
                .Replace("{Environment}", _environment)
                .Replace("{Date}", string.Empty);
    }

    /// <summary>
    ///     Check to see if the path exists for the expected file.
    ///     <para>If the path does not exist then attempt to create it.</para>
    /// </summary>
    /// <param name="filePathAndName"></param>
    /// <returns></returns>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Any exception indicates failure.")]
    private bool IsFileSystemAccessible(string filePathAndName)
    {
        if (string.IsNullOrWhiteSpace(filePathAndName)) return false;

        var dir = Path.GetDirectoryName(filePathAndName);
        if (!Directory.Exists(dir))
            // Attempt to create the directory
            try
            {
                Directory.CreateDirectory(dir);
            }
            catch
            {
                // Ignoring exception on purpose as it doesn't matter why directory creation failed
                Console.WriteLine("Unable to access directory: '{0}' for logging.", dir);
                return false;
            }

        // Attempt to check directory accessibility
        FileStream tempStream = null;
        try
        {
            tempStream = File.Create(Path.Combine(dir, string.Concat(Guid.NewGuid().ToString("N"), ".templog")), 100,
                FileOptions.DeleteOnClose);
        }
        catch
        {
            // Ignoring exception on purpose as it doesn't matter why file access is not allowed
            Console.WriteLine("Write access denied for log directory: '{0}'.", dir);
            return false;
        }
        finally
        {
            tempStream?.Close();
        }

        return true;
    }
}