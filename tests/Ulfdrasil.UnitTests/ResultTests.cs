using AwesomeAssertions;
using Xunit;

namespace Ulfdrasil.UnitTests;

public class ResultTests
{
    private class CustomResult : Result
    {
        public CustomResult(bool isSuccess, Error? error)
            : base(isSuccess, error)
        {
        }
    }

    [Fact]
    public void Success_CreatesSuccessfulResult_WithNoError()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_CreatesFailedResult_WithProvidedError()
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
    public void BaseConstructor_ThrowsInvalidOperationException_WhenSuccessHasError()
    {
        // Arrange
        var error = new Error(ErrorCode.Internal, "Error message");

        // Act & Assert
        Action act = () => _ = new CustomResult(true, error);

        act.Should()
           .Throw<InvalidOperationException>()
           .WithMessage("A successful result cannot have an error message.");
    }

    [Fact]
    public void BaseConstructor_ThrowsInvalidOperationException_WhenFailureWithoutError()
    {
        // Arrange
        var error = new Error(ErrorCode.Internal, "Error message");

        // Act & Assert
        Action act = () => _ = new CustomResult(false, null);

        act.Should()
           .Throw<InvalidOperationException>()
           .WithMessage("A failed result must have an error message.");
    }
}