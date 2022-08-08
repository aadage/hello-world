namespace TimeTrackerCore;

public abstract class CommandResult
{
    public bool IsSuccess => ValidationErrors.Count == 0;
    public List<ValidationError> ValidationErrors { get; set; } = new List<ValidationError>();
}
