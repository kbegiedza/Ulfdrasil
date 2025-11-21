using AwesomeAssertions;
using Xunit;

namespace Ulfdrasil.UnitTests;

public class ErrorTests
{
    [Fact]
    public void Constructor_WithCodeAndMessage_SetsProperties()
    {
        // Arrange
        const ErrorCode code = ErrorCode.Internal;
        const string message = "Something went wrong";

        // Act
        var error = new Error(code, message);

        // Assert
        error.Code.Should().Be(code);
        error.Message.Should().Be(message);
        error.Details.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithDetails_SetsAllProperties()
    {
        // Arrange
        const ErrorCode code = ErrorCode.Validation;
        const string message = "Validation failed";

        var details = new Dictionary<string, string[]>
        {
            ["Name"] = ["Name is required"],
            ["Age"] = ["Age must be positive"]
        };

        // Act
        var error = new Error(code, message, details);

        // Assert
        error.Code.Should().Be(code);
        error.Message.Should().Be(message);
        error.Details.Should().BeSameAs(details);
    }

    [Fact]
    public void Error_IsRecord_ProvidesValueEquality()
    {
        // Arrange
        var details = new Dictionary<string, string[]>
        {
            ["Field"] = ["Error", "Another error"]
        };

        var first = new Error(ErrorCode.Internal, "Message", details);
        var second = new Error(ErrorCode.Internal, "Message", details);

        // Assert
        first.Should().Be(second);
        (first == second).Should().BeTrue();
    }
}