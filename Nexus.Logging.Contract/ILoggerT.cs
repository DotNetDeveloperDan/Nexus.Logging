namespace Nexus.Logging.Contract
{
    /// <summary>
    /// Generic logger for a specific category name based on the <typeparamref name="TCategoryName"/>.
    /// </summary>
    /// <typeparam name="TCategoryName">The type that is used for the logger category name.</typeparam>
    public interface ILogger<out TCategoryName> : ILogger
    {
    }
}
