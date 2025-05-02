namespace Nexus.Logging;

/// <summary>
///     Provides an empty scope.
/// </summary>
public sealed class NullScope : IDisposable
{
    private NullScope()
    {
    }

    public static NullScope Instance { get; } = new();

    /// <inheritdoc />
    public void Dispose()
    {
    }
}