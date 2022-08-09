namespace TimeTrackerCore;

public class NeetRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>, IRequestPipeline<TRequest, TResponse>
{
    private IRequestHandler<TRequest, TResponse> next = null;

    public Task<TResponse> Handle(TRequest request)
    {
        if (next is null) throw new ApplicationException("next is null");
        Console.WriteLine("Neet!");
        return next.Handle(request);
    }

    public void SetNext(IRequestHandler<TRequest, TResponse> next)
    {
        this.next = next;
    }
}
