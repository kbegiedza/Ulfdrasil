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
    /// </summary>`
    /// <param name="error">The error associated with a failed operation; should be <c>null</c> for successful operations.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a successful result is created with a non-null error or when a failed result is created without an error.
    /// </exception>
    protected Result(Error? error = null)
    {
        Error = error;
        IsSuccess = error == null;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful result instance.</returns>
    public static Result Success() => new Result();

    /// <summary>
    /// Creates a successful result with the provided value.
    /// </summary>
    /// <param name="value">The value produced by the operation.</param>
    /// <returns>A successful result instance containing the provided value.</returns>
    public static Result<TValue> Success<TValue>(TValue value) => new Result<TValue>(value);

    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="error">The error describing why the operation failed.</param>
    /// <returns>A failed result instance containing the provided error.</returns>
    public static Result Failure(Error error) => new Result(error);

    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="error">The error describing why the operation failed.</param>
    /// <returns>A failed result instance containing the provided error.</returns>
    public static Result<TValue> Failure<TValue>(Error error) => new Result<TValue>(default, error);

}

/// <summary>
/// Represents the result of an operation that produces a value, indicating success or failure.
/// </summary>
/// <typeparam name="TValue">The type of the value produced on success.</typeparam>
public class Result<TValue> : Result
{
    /// <summary>
    /// Gets the value produced by a successful operation.
    /// </summary>
    public TValue? Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{TValue}"/> class.
    /// </summary>
    /// <param name="value">The value produced by the operation, if successful.</param>
    /// <param name="error">The error associated with a failed operation; should be <c>null</c> for successful operations.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a successful result is created with a non-null error or when a failed result is created without an error.
    /// </exception>
    internal Result(TValue? value, Error? error = null)
        : base(error)
    {
        Value = value;
    }
}