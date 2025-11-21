namespace Ulfdrasil;

/// <summary>
/// Represents the result of an operation, indicating success or failure.
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Error message in case of failure; null if the operation was successful.
    /// </summary>
    public Error? Error { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="isSuccess">A value indicating whether the operation was successful.</param>
    /// <param name="error">The error associated with a failed operation; should be <c>null</c> for successful operations.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a successful result is created with a non-null error or when a failed result is created without an error.
    /// </exception>
    protected Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;

        switch (isSuccess)
        {
            case true when error != null:
                throw new InvalidOperationException("A successful result cannot have an error message.");
            case false when error == null:
                throw new InvalidOperationException("A failed result must have an error message.");
        }
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful result instance.</returns>
    public static Result Success() => new Result(true, null);

    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="error">The error describing why the operation failed.</param>
    /// <returns>A failed result instance containing the provided error.</returns>
    public static Result Failure(Error error) => new Result(false, error);
}