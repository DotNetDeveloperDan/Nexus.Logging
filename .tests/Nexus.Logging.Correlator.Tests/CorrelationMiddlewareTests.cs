using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Moq;
using Nexus.Logging.Contract;
using Nexus.Logging.Correlator.Accessors;
using Nexus.Logging.Correlator.Contract;
using NUnit.Framework;

namespace Nexus.Logging.Correlator.Tests;

public class CorrelationMiddlewareTests
{
    private CorrelationMiddleware _correlationMiddleware;
    private HttpResponseFeatureMock _feature;
    private HttpContext _httpContext;
    private Mock<ICorrelationContextFactory> _mockContextFactory;
    private Mock<ILogger<CorrelationMiddleware>> _mockLogger;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<CorrelationMiddleware>>();
        _mockContextFactory = new Mock<ICorrelationContextFactory>();
        _feature = new HttpResponseFeatureMock();
        _httpContext = new DefaultHttpContext();
        _httpContext.Features.Set<IHttpResponseFeature>(_feature);

        _correlationMiddleware = new CorrelationMiddleware(
            _mockLogger.Object,
            async inner =>
            {
                await inner.Response.WriteAsync(CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
                await _feature.InvokeCallBack();
            }
        );
    }

    [Test]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new CorrelationMiddleware(null, _ => Task.CompletedTask),
            Throws.ArgumentNullException
        );
    }

    [Test]
    public void Constructor_NullNext_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new CorrelationMiddleware(_mockLogger.Object, null),
            Throws.ArgumentNullException
        );
    }

    [Test]
    public async Task InvokeAsync_VerifyParentCorrelationId()
    {
        _httpContext.Request.Headers[CorrelationHeaderKeys.CorrelationId] = "Test";
        _mockContextFactory
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .Returns(new CorrelationContext("CorrId", "Test", "123abc"));

        await _correlationMiddleware.InvokeAsync(_httpContext, _mockContextFactory.Object);

        _mockContextFactory.Verify(
            x => x.Create(It.IsAny<string>(), "Test", It.IsAny<string>(), 0),
            Times.Once
        );
    }

    [TestCase("0", 0)]
    [TestCase("NotANumber", 0)]
    [TestCase("5", 5)]
    public async Task InvokeAsync_VerifySequence(string sequence, int expectedSequence)
    {
        _httpContext.Request.Headers[CorrelationHeaderKeys.Sequence] = sequence;
        _mockContextFactory
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .Returns(new CorrelationContext("CorrId", "Test", "123abc"));

        await _correlationMiddleware.InvokeAsync(_httpContext, _mockContextFactory.Object);

        _mockContextFactory.Verify(
            x => x.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), expectedSequence),
            Times.Once
        );
    }

    [Test]
    public async Task InvokeAsync_VerifyContextDictionary()
    {
        _httpContext.Request.Headers[CorrelationHeaderKeys.CorrelationId] = "Test";
        var ctx = new CorrelationContext("CorrId", "Test", "123abc");
        _mockContextFactory
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .Returns(ctx);

        await _correlationMiddleware.InvokeAsync(_httpContext, _mockContextFactory.Object);

        _mockLogger.Verify(x => x.BeginScope(
                It.Is<Dictionary<string, object>>(d =>
                    d.Count == 4 &&
                    (string)d["CorrelationId"] == "CorrId" &&
                    (string)d["Sequence"] == "0" &&
                    (string)d["ParentCorrelationId"] == "Test" &&
                    (string)d["StackId"] == "123abc")),
            Times.Once
        );
    }

    [Test]
    public async Task InvokeAsync_VerifyContextDictionary_WithNullParentCorrelationId()
    {
        _httpContext.Request.Headers[CorrelationHeaderKeys.CorrelationId] = "Test";
        var ctx = new CorrelationContext("CorrId", null, "123abc");
        _mockContextFactory
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .Returns(ctx);

        await _correlationMiddleware.InvokeAsync(_httpContext, _mockContextFactory.Object);

        _mockLogger.Verify(x => x.BeginScope(
                It.Is<Dictionary<string, object>>(d =>
                    d.Count == 4 &&
                    (string)d["CorrelationId"] == "CorrId" &&
                    (string)d["Sequence"] == "0" &&
                    d["ParentCorrelationId"] == null &&
                    (string)d["StackId"] == "123abc")),
            Times.Once
        );
    }

    [Test]
    public async Task InvokeAsync_VerifyNewCorrelationIdHeader()
    {
        _httpContext.Request.Headers[CorrelationHeaderKeys.CorrelationId] = "Test";
        _mockContextFactory
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .Returns(new CorrelationContext("CorrId", null, "123abc"));

        await _correlationMiddleware.InvokeAsync(_httpContext, _mockContextFactory.Object);

        var newCorr = _httpContext.Request.Headers[CorrelationHeaderKeys.CorrelationId].ToString();
        Assert.That(newCorr, Is.Not.EqualTo("Test"));
    }

    [Test]
    public async Task InvokeAsync_VerifyStackIdGeneratedAndAddedToRequestAndResponseHeadersWhenNotIncludedInRequest()
    {
        _httpContext.Request.Headers[CorrelationHeaderKeys.CorrelationId] = "Test";

        await _correlationMiddleware.InvokeAsync(
            _httpContext,
            new CorrelationContextFactory(new CorrelationContextAccessor())
        );

        var req = _httpContext.Request.Headers[CorrelationHeaderKeys.StackId].ToString();
        var res = _httpContext.Response.Headers[CorrelationHeaderKeys.StackId].ToString();

        Assert.That(req, Is.Not.Null.And.Not.Empty);
        Assert.That(res, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public async Task InvokeAsync_VerifyStackIdHeaderProvidedInRequestHeaderRemainsUnchangedInResponseHeader()
    {
        const string value = "IShouldBeInTheResponseHeaders";
        _httpContext.Request.Headers[CorrelationHeaderKeys.StackId] = value;

        await _correlationMiddleware.InvokeAsync(
            _httpContext,
            new CorrelationContextFactory(new CorrelationContextAccessor())
        );

        Assert.That(
            _httpContext.Request.Headers[CorrelationHeaderKeys.StackId].ToString(),
            Is.EqualTo(value)
        );
        Assert.That(
            _httpContext.Response.Headers[CorrelationHeaderKeys.StackId].ToString(),
            Is.EqualTo(value)
        );
    }
}