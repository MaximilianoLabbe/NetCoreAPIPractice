namespace API.Data.Exceptions;

public class DuplicateKeyException : Exception
{
    public string Entity { get; }

    public string Field { get; }

    public object? Value { get; }

    public DuplicateKeyException(
        string entity,
        string field,
        object? value,
        Exception innerException)
        : base(
            $"Duplicate value detected for {entity}.{field}.",
            innerException)
    {
        Entity = entity;
        Field = field;
        Value = value;
    }
}