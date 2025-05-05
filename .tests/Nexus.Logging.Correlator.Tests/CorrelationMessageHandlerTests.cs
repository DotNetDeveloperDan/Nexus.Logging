using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Nexus.Logging.Correlator.Contract;
using NUnit.Framework;

namespace Nexus.Logging.Correlator.Tests
{
    public class CorrelationMessageHandlerTests
    {
        private Mock<ICorrelationContextAccessor> _mockContextAccessor;
        private CorrelationMessageHandlerMock _handler;

        [SetUp]
        public void Setup()
        {
            _mockContextAccessor = new Mock<ICorrelationContextAccessor>();
            _handler = new CorrelationMessageHandlerMock(_mockContextAccessor.Object);
        }

        [Test]
        public async Task SendAsync_Headers_ReturnsCorrelationIdAndSequence()
        {
            var request = new HttpRequestMessage();
            _mockContextAccessor
                .Setup(x => x.CurrentContext)
                .Returns(new CorrelationContext("1", "1", "123abc"));

            await _handler.SendAsync(request);

            Assert.That(
                request.Headers.GetValues(CorrelationHeaderKeys.Sequence).FirstOrDefault(),
                Is.EqualTo("1")
            );
            Assert.That(
                request.Headers.GetValues(CorrelationHeaderKeys.CorrelationId).FirstOrDefault(),
                Is.EqualTo("1")
            );
            Assert.That(
                request.Headers.GetValues(CorrelationHeaderKeys.RequestId).FirstOrDefault(),
                Is.EqualTo("123abc")
            );
        }

        [Test]
        public async Task SendAsync_ExistingHeaders_ReturnsOverriddenHeaderData()
        {
            var request = new HttpRequestMessage();
            request.Headers.Add(CorrelationHeaderKeys.CorrelationId, "TestValue");
            _mockContextAccessor
                .Setup(x => x.CurrentContext)
                .Returns(new CorrelationContext("TestValue2", "1", "123abc"));

            await _handler.SendAsync(request);

            Assert.That(
                request.Headers.GetValues(CorrelationHeaderKeys.CorrelationId).FirstOrDefault(),
                Is.EqualTo("TestValue2")
            );
        }

        [TestCase("")]
        [TestCase(null)]
        public async Task SendAsync_EmptyOrNullCorrelation_DoNotAddToHeaders(string corrId)
        {
            var request = new HttpRequestMessage();
            _mockContextAccessor
                .Setup(x => x.CurrentContext)
                .Returns(new CorrelationContext(corrId, "1", "123abc"));

            await _handler.SendAsync(request);

            Assert.That(
                request.Headers.Contains(CorrelationHeaderKeys.CorrelationId),
                Is.False
            );
        }
    }
}
