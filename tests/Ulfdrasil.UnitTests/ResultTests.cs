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

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();

        result.Problem.Should().BeNull();
    }

    [Fact]
    public void Success_CreatesSuccessfulResult_WithoutValue()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Problem.Should().BeNull();
    }

    [Fact]
    public void Failure_CreatesFailedResult_WithError()
    {
        // Arrange
        var error = new Problem("error", "Something went wrong");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();

        result.Problem.Should().BeSameAs(error);
    }

    [Fact]
    public void Failure_CreatesFailedResultGeneric_WithError()
    {
        // Arrange
        var error = new Problem("error", "Something went wrong");

        // Act
        var result = Result.Failure<int>(error);

        // Assert
        result.Should().BeOfType<Result<int>>();

        result.Value.Should().Be(default);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();

        result.Problem.Should().BeSameAs(error);
    }
}