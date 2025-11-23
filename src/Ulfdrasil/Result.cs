using System.Diagnostics.CodeAnalysis;

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
    /// Indicates whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Error message in case of failure; null if the operation was successful.
    /// </summary>
    public FailureReason? FailureReason { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>`
    /// <param name="failureReason">The error associated with a failed operation; should be <c>null</c> for successful operations.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a successful result is created with a non-null error or when a failed result is created without an error.
    /// </exception>
    protected Result(FailureReason? failureReason = null)
    {
        FailureReason = failureReason;
        IsSuccess = failureReason == null;
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
    /// <param name="failureReason">The error describing why the operation failed.</param>
    /// <returns>A failed result instance containing the provided error.</returns>
    public static Result Failure(FailureReason failureReason) => new Result(failureReason);

    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="failureReason">The error describing why the operation failed.</param>
    /// <returns>A failed result instance containing the provided error.</returns>
    public static Result<TValue> Failure<TValue>(FailureReason failureReason) => new Result<TValue>(failureReason);
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
    [MaybeNull]
    [MemberNotNullWhen(true, nameof(HasValue))]
    public TValue? Value { get; }

    internal bool HasValue => FailureReason == null;

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{TValue}"/> class.
    /// </summary>
    /// <param name="value">The value produced by the operation, if successful.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a successful result is created with a non-null error or when a failed result is created without an error.
    /// </exception>
    internal Result(TValue value)
    {
        Value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{TValue}"/> class.
    /// </summary>
    /// <param name="failureReason">The error associated with a failed operation; should be <c>null</c> for successful operations.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a successful result is created with a non-null error or when a failed result is created without an error.
    /// </exception>
    internal Result(FailureReason failureReason)
        : base(failureReason)
    {
        // is it safe to set default for non-nullable types?
        Value = default;
    }
}