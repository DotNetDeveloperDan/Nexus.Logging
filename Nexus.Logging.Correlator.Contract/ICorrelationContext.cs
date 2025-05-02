namespace Nexus.Logging.Correlator.Contract;

/// <summary>
///     The data that is used for context between application -> service calls.
///     <para>Should be accessed in a request through the <see cref="ICorrelationContextAccessor" />.</para>
/// </summary>
public interface ICorrelationContext
{
    /// <summary>
    ///     Contains data for <see cref="CorrelationHeaderKeys.CorrelationId" />
    /// </summary>
    string CorrelationId { get; }

    /// <summary>
    ///     Contains data for <see cref="CorrelationHeaderKeys.Sequence" />
    /// </summary>
    int Sequence { get; }

    /// <summary>
    ///     Contains data for <see cref="CorrelationHeaderKeys.ParentCorrelationId" />.
    ///     <para>Used when sequencing could be affected beyond the current scope.</para>
    /// </summary>
    string ParentCorrelationId { get; }

    /// <summary>
    ///     Contains an identifier that can be used for tracing from <see cref="CorrelationHeaderKeys.StackId" />
    ///     Called StackId as it identifies the request across application call stacks.
    /// </summary>
    string StackId { get; }

    /// <summary>
    ///     Increments <see cref="CorrelationHeaderKeys.Sequence" />
    /// </summary>
    void IncrementSequence();
}