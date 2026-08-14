namespace ModularMonolith.Template.SharedKernel.Results;

public sealed record Error
{
    #region Static Field Declarations

    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    #endregion

    #region Property Declarations

    public string Code { get; }

    public string Message { get; }

    public ErrorType Type { get; }

    #endregion

    #region Constructor Declerations

    public Error(string code, string message, ErrorType type)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(message);

        Code = code;
        Message = message;
        Type = type;
    }

    #endregion

    #region Public Method Declarations

    public static Error Failure(string code, string message) => new(code, message, ErrorType.Failure);

    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);

    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);

    public static Error Unauthorised(string code, string message) => new(code, message, ErrorType.Unauthorised);

    public static Error Forbidden(string code, string message) => new(code, message, ErrorType.Forbidden);

    #endregion
}
