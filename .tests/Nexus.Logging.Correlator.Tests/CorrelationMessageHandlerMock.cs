using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Logging.Correlator.Contract;
using Nexus.Logging.Correlator.MessageHandlers;

namespace Nexus.Logging.Correlator.Tests;

public class CorrelationMessageHandlerMock : CorrelationMessageHandler
{
    public CorrelationMessageHandlerMock(ICorrelationContextAccessor correlationContext) : base(correlationContext)
    {
        InnerHandler = new TestHandler();
    }

    internal Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        return base.SendAsync(request, CancellationToken.None);
    }
}

public class TestHandler : DelegatingHandler
{
    private readonly Func<HttpRequestMessage,
        CancellationToken, Task<HttpResponseMessage>> _handlerFunc;

    public TestHandler()
    {
        _handlerFunc = (r, c) => Return200();
    }

    public TestHandler(Func<HttpRequestMessage,
        CancellationToken, Task<HttpResponseMessage>> handlerFunc)
    {
        _handlerFunc = handlerFunc;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return _handlerFunc(request, cancellationToken);
    }

    public static Task<HttpResponseMessage> Return200()
    {
        return Task.Factory.StartNew(() => new HttpResponseMessage(HttpStatusCode.OK));
    }
}