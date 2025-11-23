using AwesomeAssertions;
using Xunit;

namespace Ulfdrasil.UnitTests;

public class FailureReasonTests
{
    [Fact]
    public void Constructor_WithCodeAndMessage_SetsProperties()
    {
        // Arrange
        const string code = "error_code";
        const string message = "Something went wrong";

        // Act
        var error = new FailureReason(code, message);

        // Assert
        error.Code.Should().Be(code);
        error.Description.Should().Be(message);
        error.Details.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithDetails_SetsAllProperties()
    {
        // Arrange
        const string code = "error_code";
        const string message = "Validation failed";

        var details = new Dictionary<string, string[]>
        {
            ["Name"] = ["Name is required"],
            ["Age"] = ["Age must be positive"]
        };

        // Act
        var error = new FailureReason(code, message, details);

        // Assert
        error.Code.Should().Be(code);
        error.Description.Should().Be(message);
        error.Details.Should().BeSameAs(details);
    }

    [Fact]
    public void Error_IsRecord_ProvidesValueEquality()
    {
        // Arrange
        const string code = "error1";
        const string message = "Message";

        var details = new Dictionary<string, string[]>
        {
            ["Field"] = ["Error", "Another error"]
        };

        var first = new FailureReason(code, message, details);
        var second = new FailureReason(code, message, details);

        // Assert
        first.Should().Be(second);
        (first == second).Should().BeTrue();
    }
}