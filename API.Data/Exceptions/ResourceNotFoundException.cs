namespace API.Business.Exceptions;

public class ResourceNotFoundException : Exception
{
    public string Resource { get; }

    public object Key { get; }

    public ResourceNotFoundException(
        string resource,
        object key)
        : base($"{resource} with identifier '{key}' was not found.")
    {
        Resource = resource;
        Key = key;
    }
}