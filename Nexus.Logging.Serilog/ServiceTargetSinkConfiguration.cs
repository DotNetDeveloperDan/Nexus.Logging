using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Nexus.Logging.Configuration;
using Serilog;
using Serilog.Configuration;
using Serilog.Formatting;

namespace Nexus.Logging.Serilog;

/// <summary>
///     Dynamic configuration for additional Serilog Sinks.
/// </summary>
internal static class ServiceTargetSinkConfiguration
{
    /// <summary>
    ///     Dynamically loads and registers a <see cref="LoggerTarget.Service" /> from the Sinks library based on
    ///     <see cref="LoggerTargetOptions.Args" />.
    ///     <para><see cref="LoggerTargetOptions.Args" />.Using -> AssemblyName to load.</para>
    ///     <para><see cref="LoggerTargetOptions.Args" />.Name -> Sink name and expected configuration method name.</para>
    ///     <para>Similar to the methodology used in Serilog.Settings.Configuration.</para>
    ///     <para>Currently only supports dynamically loading if an extension method matching the Name is found.</para>
    /// </summary>
    /// <param name="targetOptions"></param>
    /// <param name="formatter"></param>
    /// <param name="receiver">
    ///     <see cref="LoggerConfiguration.WriteTo" />
    /// </param>
    internal static void Register(LoggerTargetOptions targetOptions, ITextFormatter formatter, object receiver)
    {
        var sink = LoadSink(targetOptions);
        var sinkConfigMethods = FindSinkConfigurationMethods(sink.Assembly);
        if (sinkConfigMethods == null || sinkConfigMethods.Count == 0)
            throw new LoggerConfigurationException("Unable to find configuration method for ");

        // Cast Target:Args and add the formatter as an argument to enable binding in the selectedMethod
        IDictionary<string, object> targetArgs = targetOptions.Args.ToDictionary(k => k.Key, v => (object)v.Value);
        targetArgs.Add("Formatter", formatter);

        var selectedMethod =
            SelectConfigurationMethod(sinkConfigMethods, sink.Name, targetArgs.Select(s => s.Key).ToList());
        if (selectedMethod != null)
        {
            var call = (from p in selectedMethod.GetParameters().Skip(1)
                    let arg = targetArgs.FirstOrDefault(s => s.Key.Equals(p.Name, StringComparison.OrdinalIgnoreCase))
                    select arg.Key == null
                        ? p.HasDefaultValue
                            ? p.DefaultValue
                            : throw new LoggerConfigurationException(
                                $"Target Args missing parameter: '{p.Name}' for Name: '{sink.Name}'")
                        : arg.Value)
                .ToList();

            call.Insert(0, receiver);
            try
            {
                selectedMethod.Invoke(null, call.ToArray());
            }
            catch (Exception ex)
            {
                // There are too many different exceptions that can occur when reflection invoking so total failure is assumed
                throw new LoggerConfigurationException(
                    $"Failure occured while registering service target with Name: {sink.Name}", ex);
            }
        }
    }

    /// <summary>
    ///     Get the name and load the sink assembly specified in <see cref="LoggerTargetOptions.Args" />.
    /// </summary>
    /// <param name="targetOptions"></param>
    /// <returns></returns>
    private static (string Name, Assembly Assembly) LoadSink(LoggerTargetOptions targetOptions)
    {
        // Need to check for a required parameter
        var sinkName = targetOptions.Args.GetValueOrDefault<string>("Name", null);
        if (string.IsNullOrWhiteSpace(sinkName))
            throw new LoggerConfigurationException("Target Args missing 'Name' parameter");

        var sinkAssemblyName = targetOptions.Args.GetValueOrDefault<string>("Using", null);
        if (string.IsNullOrWhiteSpace(sinkAssemblyName))
            throw new LoggerConfigurationException("Target Args missing 'Using' parameter");
        return (sinkName, Assembly.Load(new AssemblyName(sinkAssemblyName)));
    }

    #region Adapted from Serilog.Settings.Configuration

    /****
     * The following methods were adapted from Serilog.Settings.Configuration.
     * https://github.com/serilog/serilog-settings-configuration.git
     * Since we are not using the same configuration syntax as Serilog expects they have
     * been modified to work with the Nexus.Logging.Configuration options.
     ****/


    /// <summary>
    ///     Find extension methods in the Sink assembly to use for configuration.
    ///     <para>Extension method configuration is the desired pattern for Serilog.</para>
    /// </summary>
    /// <param name="sinkAssembly"></param>
    /// <returns></returns>
    private static List<MethodInfo> FindSinkConfigurationMethods(Assembly sinkAssembly)
    {
        return sinkAssembly
            .GetExportedTypes()
            .Select(s => s.GetTypeInfo())
            .Where(w => w.IsSealed && w.IsAbstract && !w.IsNested)
            .SelectMany(t => t.DeclaredMethods)
            .Where(w => w.IsStatic && w.IsDefined(typeof(ExtensionAttribute), false))
            .Where(m => m.GetParameters()[0].ParameterType == typeof(LoggerSinkConfiguration))
            .ToList();
    }

    /// <summary>
    ///     Determines which configuration method is suitable to use based on <see cref="LoggerTargetOptions.Args" /> values
    ///     that match method parameters.
    /// </summary>
    /// <param name="candidateMethods">All available configuration methods that were found.</param>
    /// <param name="sinkName">The name of the sink to help filter through the found candidateMethods.</param>
    /// <param name="argumentNames">The names of all configured arguments to help filter through method signatures.</param>
    /// <returns></returns>
    private static MethodInfo SelectConfigurationMethod(List<MethodInfo> candidateMethods, string sinkName,
        List<string> argumentNames)
    {
        var selectedMethod = candidateMethods
            .Where(m => m.Name == sinkName)
            .Where(m => m.GetParameters()
                .Skip(1)
                .All(p => p.HasDefaultValue || p.ParameterType == typeof(IConfiguration)
                                            || argumentNames.Any(args =>
                                                args.Equals(p.Name, StringComparison.OrdinalIgnoreCase))))
            .OrderByDescending(m =>
            {
                var matchingArgs = m.GetParameters()
                    .Where(p => argumentNames.Any(args => args.Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                // Rank based on the most number of matches and the most number of matches that are of type string
                return new Tuple<int, int>(
                    matchingArgs.Count,
                    matchingArgs.Count(p => p.ParameterType == typeof(string)));
            })
            .FirstOrDefault();

        if (selectedMethod == null)
        {
            var methodsByName = candidateMethods
                .Where(m => m.Name == sinkName)
                .Select(m => $"{m.Name}({string.Join(", ", m.GetParameters().Skip(1).Select(p => p.Name))})")
                .ToList();
        }

        return selectedMethod;
    }

    #endregion
}