namespace API.Business.Exceptions;

public class DuplicateResourceException : Exception
{
    public string Resource { get; }

    public string Field { get; }

    public object? Value { get; }

    public DuplicateResourceException(
        string resource,
        string field,
        object? value)
        : base(
            $"{resource} with {field} '{value}' already exists.")
    {
        Resource = resource;
        Field = field;
        Value = value;
    }
}