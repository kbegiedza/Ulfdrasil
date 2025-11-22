using AwesomeAssertions;
using Xunit;

namespace Ulfdrasil.UnitTests;

public class ResultTests
{
    [Fact]
    public void Success_CreatesSuccessfulResultGeneric_WithValue()
    {
        // Arrange
        const int expectedValue = 420;

        // Act
        var result = Result.Success(expectedValue);

        // Assert
        result.Value.Should().Be(expectedValue);
        result.HasValue.Should().BeTrue();
        result.Succeeded.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Success_CreatesSuccessfulResult_WithoutValue()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_CreatesFailedResult_WithError()
    {
        // Arrange
        var error = new Error(ErrorCode.Internal, "Something went wrong");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().BeSameAs(error);
    }

    [Fact]
    public void Failure_CreatesFailedResultGeneric_WithError()
    {
        // Arrange
        var error = new Error(ErrorCode.Internal, "Something went wrong");

        // Act
        var result = Result.Failure<int>(error);

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.HasValue.Should().BeFalse();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().BeSameAs(error);
    }
}