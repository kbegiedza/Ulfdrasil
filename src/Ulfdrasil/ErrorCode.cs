namespace Ulfdrasil;

/// <summary>
/// Standardized error codes for operation results.
/// </summary>
public enum ErrorCode
{
    /// <summary>
    /// Undefined error code.
    /// </summary>
    Undefined = -1,

    /// <summary>
    /// The operation is invalid in the current context.
    /// </summary>
    InvalidOperation = 400,

    /// <summary>
    /// Unauthorized to perform the operation.
    /// </summary>
    Unauthorized = 401,

    /// <summary>
    /// The operation is forbidden.
    /// </summary>
    Forbidden = 403,

    /// <summary>
    /// The requested resource was not found.
    /// </summary>
    NotFound = 404,

    /// <summary>
    /// A conflict occurred with the current state of the resource.
    /// </summary>
    Conflict = 409,

    /// <summary>
    /// Validation failed.
    /// </summary>
    Validation = 422,

    /// <summary>
    /// An internal server error occurred.
    /// </summary>
    Internal = 500
}