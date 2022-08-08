namespace TimeTrackerCore;

public interface IValidator<T>
{
    List<ValidationError> Validate(T value);
}

public class ValidationError
{
    public string? MemberName { get; set; } = null;
    public string Message { get; set; } = string.Empty;
}
