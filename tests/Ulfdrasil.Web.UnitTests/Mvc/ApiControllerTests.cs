using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Xunit;
using Ulfdrasil.Web.Mvc;

namespace Ulfdrasil.Web.UnitTests.Mvc;

public class ApiControllerTests
{
    private const string ExpectedMessage = "Test log message";

    public class TestController : ApiController
    {
        public TestController(ILogger<TestController> logger)
            : base(logger)
        {
        }

        public void TestMethod()
        {
            Logger.LogInformation(ExpectedMessage);
        }
    }

    [Fact]
    public void Should_Initialize_Logger()
    {
        // Arrange
        var logger = new FakeLogger<TestController>();
        var controller = new TestController(logger);

        // Act
        controller.TestMethod();

        // Assert
        logger.Collector.GetSnapshot()
              .Should()
              .ContainSingle(log => log.Message == ExpectedMessage);

    }

}