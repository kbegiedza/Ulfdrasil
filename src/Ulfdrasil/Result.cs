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
    private readonly bool _isSuccess;

    /// <summary>
    /// Error message in case of failure; null if the operation was successful.
    /// </summary>
    private readonly Problem? _problem;

    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Problem))]
    public virtual bool IsSuccess => _isSuccess;

    /// <summary>
    /// Indicates whether the operation failed.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Problem))]
    public virtual bool IsFailure => !_isSuccess;

    /// <summary>
    /// Error message in case of failure; null if the operation was successful.
    /// </summary>
    public virtual Problem? Problem => _problem;

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>`
    /// <param name="problem">The error associated with a failed operation; should be <c>null</c> for successful operations.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a successful result is created with a non-null error or when a failed result is created without an error.
    /// </exception>
    protected Result(Problem? problem = null)
    {
        _problem = problem;
        _isSuccess = problem is null;
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
    /// <param name="problem">The error describing why the operation failed.</param>
    /// <returns>A failed result instance containing the provided error.</returns>
    public static Result Failure(Problem problem) => new Result(problem);

    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="problem">The error describing why the operation failed.</param>
    /// <returns>A failed result instance containing the provided error.</returns>
    public static Result<TValue> Failure<TValue>(Problem problem) => new Result<TValue>(problem);
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
    /// Indicates whether the operation was successful.
    /// <remarks>
    /// Overriden to satisfy nullable analysis for Value.
    /// </remarks>
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Problem))]
    public override bool IsSuccess => base.IsSuccess;

    /// <summary>
    /// Indicates whether the operation failed.
    /// <remarks>
    /// Overriden to satisfy nullable analysis for Problem.
    /// </remarks>
    /// </summary>
    [MemberNotNullWhen(false, nameof(Value))]
    [MemberNotNullWhen(true, nameof(Problem))]
    public override bool IsFailure => base.IsFailure;

    // ReSharper disable once RedundantOverriddenMember : Disabled to satisfy nullable analysis for Problem.
    /// <summary>
    /// Error message in case of failure; null if the operation was successful.
    /// </summary>
    public override Problem? Problem => base.Problem;

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
    /// <param name="problem">The error associated with a failed operation; should be <c>null</c> for successful operations.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a successful result is created with a non-null error or when a failed result is created without an error.
    /// </exception>
    internal Result(Problem problem)
        : base(problem)
    {
        Value = default;
    }
}