using Moq;
using Nexus.Logging.Correlator.Accessors;
using Nexus.Logging.Correlator.Contract;
using NUnit.Framework;

namespace Nexus.Logging.Correlator.Tests;

public class CorrelationContextFactoryTests
{
    private CorrelationContextFactory _factory;
    private Mock<ICorrelationContextAccessor> _mockContextAccessor;

    [SetUp]
    public void Setup()
    {
        _mockContextAccessor = new Mock<ICorrelationContextAccessor>();
        _factory = new CorrelationContextFactory(_mockContextAccessor.Object);
    }

    [Test]
    public void Create_ReturnsCorrelationContext()
    {
        var ctx = _factory.Create("1", "0", "123abc");

        Assert.That(ctx.CorrelationId, Is.EqualTo("1"));
        Assert.That(ctx.ParentCorrelationId, Is.EqualTo("0"));
        Assert.That(ctx.StackId, Is.EqualTo("123abc"));
    }

    [Test]
    public void Create_VerifyCorrelationContextAccessor()
    {
        var realAccessor = new CorrelationContextAccessor();
        _factory = new CorrelationContextFactory(realAccessor);

        var ctx = _factory.Create("1", "0", "123abc");

        Assert.That(realAccessor.CurrentContext, Is.SameAs(ctx));
    }

    [Test]
    public void Create_VerifyDispose()
    {
        var realAccessor = new CorrelationContextAccessor();
        _factory = new CorrelationContextFactory(realAccessor);

        var ctx = _factory.Create("1", "0", "123abc");
        Assert.That(realAccessor.CurrentContext, Is.SameAs(ctx));

        _factory.Dispose();
        Assert.That(realAccessor.CurrentContext, Is.Null);
    }
}