namespace API.Business.Exceptions;

public class BusinessValidationException : Exception
{
    public string Field { get; }

    public object? Value { get; }

    public BusinessValidationException(
        string field,
        object? value,
        string message)
        : base(message)
    {
        Field = field;
        Value = value;
    }
}