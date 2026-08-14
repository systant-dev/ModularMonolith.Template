namespace ModularMonolith.Template.SharedKernel.Results;

public sealed class Result<TValue> : Result
{
    #region Field Declarations

    private readonly TValue? _value;

    #endregion

    #region Property Declarations

    public TValue Value => IsSuccess ? _value! : throw new InvalidOperationException("The value of a failed result cannot be accessed.");

    #endregion

    #region Constructor Declarations

    internal Result(TValue? value, bool isSuccess, Error error) : base(isSuccess, error)
    {
        _value = value;
    }

    #endregion

    #region Public Method Declarations

    public static implicit operator Result<TValue>(TValue value) => Success(value);

    public static implicit operator Result<TValue>(Error error) => Failure<TValue>(error);

    #endregion
}
