using AwesomeAssertions;
using Xunit;

namespace Ulfdrasil.UnitTests;

public class ResultTests
{
    [Theory]
    [InlineData(420)]
    [InlineData(true)]
    [InlineData("test text")]
    public void Success_CreatesSuccessfulResult_WithValue(object value)
    {
        // Act
        var result = Result.Success(value);

        // Assert
        result.Value.Should().Be(value);
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Success_CreatesSuccessfulResult_WithoutValue()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
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
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeSameAs(error);
    }

    [Fact]
    public void Failure_CreatesFailedResult_WithValueAndError()
    {
        // Arrange
        var error = new Error(ErrorCode.Internal, "Something went wrong");

        // Act
        var result = Result.Failure<int>(error);

        // Assert
        result.Should().BeOfType<Result<int>>();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeSameAs(error);
    }
}