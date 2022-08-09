namespace TimeTrackerCore;

public class CommandTimerRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>, IRequestPipeline<TRequest, TResponse>
{
    private IRequestHandler<TRequest, TResponse> next = null;

    public Task<TResponse> Handle(TRequest request)
    {
        if (next is null) throw new ApplicationException("next not set");
        DateTime start = DateTime.Now;
        DateTime end;
        TimeSpan elapsed;

        Console.WriteLine($"Command Start: {start.ToString()}");
        var response = next.Handle(request);
        end = DateTime.Now;
        elapsed = end.Subtract(start);
        Console.WriteLine($"Command End: {end.ToString()}");
        Console.WriteLine($"Elapsed Time: {elapsed.ToString()}");

        return response;
    }

    public void SetNext(IRequestHandler<TRequest, TResponse> next)
    {
        this.next = next;
    }
}
