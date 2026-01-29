using Ulfdrasil;

namespace Ulfdrasil.Hyperbatch.Internal;

internal sealed class HyperbatchWorkItem<TRequest, TResponse>
{
    private readonly CancellationTokenRegistration _registration;

    public HyperbatchWorkItem(
        HyperbatchRequest<TRequest> request,
        int tokenCount,
        DateTimeOffset enqueuedAt,
        CancellationToken cancellationToken)
    {
        Request = request;
        TokenCount = tokenCount;
        EnqueuedAt = enqueuedAt;
        Deadline = request.Deadline;
        CancellationToken = cancellationToken;
        Completion = new TaskCompletionSource<Result<TResponse>>(TaskCreationOptions.RunContinuationsAsynchronously);

        if (cancellationToken.CanBeCanceled)
        {
            _registration = cancellationToken.Register(
                static state => ((HyperbatchWorkItem<TRequest, TResponse>)state!).TrySetCanceled(),
                this);
        }
    }

    public HyperbatchRequest<TRequest> Request { get; }

    public TaskCompletionSource<Result<TResponse>> Completion { get; }

    public CancellationToken CancellationToken { get; }

    public DateTimeOffset EnqueuedAt { get; }

    public int TokenCount { get; }

    public DateTimeOffset? Deadline { get; }

    public bool TrySetResult(Result<TResponse> result)
    {
        if (CancellationToken.IsCancellationRequested)
        {
            return TrySetCanceled();
        }

        var set = Completion.TrySetResult(result);
        _registration.Dispose();
        return set;
    }

    public bool TrySetCanceled()
    {
        var set = CancellationToken.CanBeCanceled
            ? Completion.TrySetCanceled(CancellationToken)
            : Completion.TrySetCanceled();
        _registration.Dispose();
        return set;
    }
}
