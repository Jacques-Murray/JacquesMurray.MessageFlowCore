namespace JacquesMurray.MessageFlowCore.Abstractions;

using Messages;

/// <summary>
/// Interface for the mediator pattern implementation.
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Sends a request to a single handler and returns the response.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="request">The request object.</param>
    /// <returns>The response from the handler.</returns>
    TResponse Send<TResponse>(IRequest<TResponse> request);

    /// <summary>
    /// Sends a request to a single handler asynchronously and returns the response.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response.</returns>
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a notification to multiple handlers.
    /// </summary>
    /// <param name="notification">The notification.</param>
    void Publish(INotification notification);

    /// <summary>
    /// Publishes a notification to multiple handlers asynchronously.
    /// </summary>
    /// <param name="notification">The notification.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task PublishAsync(INotification notification, CancellationToken cancellationToken = default);
}