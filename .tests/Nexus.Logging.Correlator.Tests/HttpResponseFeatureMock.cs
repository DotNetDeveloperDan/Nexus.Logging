using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Nexus.Logging.Correlator.Tests;

/// <summary>
///     Provides a way to invoke the HttpContext.Response.OnStarting callback in middleware.
/// </summary>
public class HttpResponseFeatureMock : IHttpResponseFeature
{
    private Func<object, Task> _onStartingCallback;
    private object _onStartingState;
    public int StatusCode { get; set; }
    public string ReasonPhrase { get; set; }
    public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
    public Stream Body { get; set; } = Stream.Null;
    public bool HasStarted { get; set; }

    public void OnStarting(Func<object, Task> callback, object state)
    {
        _onStartingCallback = callback;
        _onStartingState = state;
    }

    public void OnCompleted(Func<object, Task> callback, object state)
    {
    }

    /// <summary>
    ///     Invokes the registered OnStarting callback.
    /// </summary>
    /// <returns></returns>
    public Task InvokeCallBack()
    {
        HasStarted = true;
        return _onStartingCallback(_onStartingState);
    }
}