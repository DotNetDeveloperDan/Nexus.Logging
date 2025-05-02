using SysEnv = System.Environment;

namespace Nexus.Logging.Configuration;

/// <summary>
///     Global application scope properties to include with log messages.
/// </summary>
public sealed class ApplicationScopeOptions
{
    /// <summary>
    ///     The application name to report in logs.
    /// </summary>
    public string ApplicationName { get; set; }

    /// <summary>
    ///     The environment the application is running in that should be reported in logs.
    /// </summary>
    public string Environment { get; set; }

    /// <summary>
    ///     When running in docker this will be populated with a GUID to identify the container which can reset.
    /// </summary>
    public string MachineName { get; } = SysEnv.MachineName;

    /// <summary>
    ///     Some static identifier that should be re-evaluated as to the meaning.
    /// </summary>
    public string InstanceId { get; } = Guid.NewGuid().ToString();

    /// <summary>
    ///     The username when user logged in to OS.
    ///     <para>Most likely will return the username tied to AppPool or service registration.</para>
    /// </summary>
    public string UserName { get; } = SysEnv.UserName;

    /// <summary>
    ///     Helper method to convert to a dictionary for usage as a Scope.
    /// </summary>
    /// <returns></returns>
    public IDictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>
        {
            { nameof(ApplicationName), ApplicationName },
            { nameof(Environment), Environment },
            { nameof(MachineName), MachineName },
            { nameof(InstanceId), InstanceId },
            { nameof(UserName), UserName }
        };
    }
}