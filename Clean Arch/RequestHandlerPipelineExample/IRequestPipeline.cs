namespace TimeTrackerCore;

public interface IRequestPipeline<TRequest, TResponse>
{
    void SetNext(IRequestHandler<TRequest, TResponse> next);
}
