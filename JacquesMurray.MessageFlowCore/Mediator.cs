namespace JacquesMurray.MessageFlowCore;

using Abstractions;
using Abstractions.Handlers;
using Abstractions.Messages;
using System.Reflection;

/// <summary>
/// Default implementation of the <see cref="IMediator"/> interface.
/// </summary>
public sealed class Mediator : IMediator
{
    private readonly ServiceContainer _serviceContainer;

    /// <summary>
    /// Initializes a new instance of the <see cref="Mediator"/> class.
    /// </summary>
    /// <param name="serviceContainer">The service container used for resolving handlers.</param>
    public Mediator(ServiceContainer serviceContainer)
    {
        _serviceContainer = serviceContainer ?? throw new ArgumentNullException(nameof(serviceContainer));
    }

    /// <summary>
    /// Sends a request to a single handler and returns the response.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="request">The request object.</param>
    /// <returns>The response from the handler.</returns>
    public TResponse Send<TResponse>(IRequest<TResponse> request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        try
        {
            dynamic? handler = _serviceContainer.Resolve(handlerType);
            if (handler == null)
            {
                throw new InvalidOperationException($"No handler registered for request type: {requestType.Name}");
            }
            return handler.Handle((dynamic)request);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException($"No handler registered for request type: {requestType.Name}");
        }
    }

    /// <summary>
    /// Sends a request to a single handler asynchronously and returns the response.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response.</returns>
    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var requestType = request.GetType();
        var handlerType = typeof(IAsyncRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        try
        {
            dynamic? handler = _serviceContainer.Resolve(handlerType);
            if (handler == null)
            {
                throw new InvalidOperationException($"No async handler registered for request type: {requestType.Name}");
            }
            return await handler.HandleAsync((dynamic)request, cancellationToken);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException($"No async handler registered for request type: {requestType.Name}");
        }
    }

    /// <summary>
    /// Publishes a notification to multiple handlers.
    /// </summary>
    /// <param name="notification">The notification.</param>
    public void Publish(INotification notification)
    {
        if (notification == null) throw new ArgumentNullException(nameof(notification));

        var notificationType = notification.GetType();
        var handlerType = typeof(INotificationHandler<>).MakeGenericType(notificationType);
        var handlers = _serviceContainer.ResolveAll(handlerType);

        foreach (dynamic handler in handlers)
        {
            if (handler != null)
            {
                handler.Handle((dynamic)notification);
            }
        }
    }

    /// <summary>
    /// Publishes a notification to multiple handlers asynchronously.
    /// </summary>
    /// <param name="notification">The notification.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task PublishAsync(INotification notification, CancellationToken cancellationToken = default)
    {
        if (notification == null) throw new ArgumentNullException(nameof(notification));

        var notificationType = notification.GetType();
        var handlerType = typeof(IAsyncNotificationHandler<>).MakeGenericType(notificationType);
        var handlers = _serviceContainer.ResolveAll(handlerType);

        var tasks = new List<Task>();
        foreach (dynamic handler in handlers)
        {
            if (handler != null)
            {
                tasks.Add(handler.HandleAsync((dynamic)notification, cancellationToken));
            }
        }

        await Task.WhenAll(tasks);
    }
}