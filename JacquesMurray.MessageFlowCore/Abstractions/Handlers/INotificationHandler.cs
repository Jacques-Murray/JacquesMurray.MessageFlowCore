namespace JacquesMurray.MessageFlowCore.Abstractions.Handlers;

using Messages;

/// <summary>
/// Interface for handling notification messages.
/// </summary>
/// <typeparam name="TNotification">The type of the notification.</typeparam>
public interface INotificationHandler<in TNotification>
    where TNotification : INotification
{
    /// <summary>
    /// Handles the specified notification.
    /// </summary>
    /// <param name="notification">The notification.</param>
    void Handle(TNotification notification);
}

/// <summary>
/// Interface for handling asynchronous notification messages.
/// </summary>
/// <typeparam name="TNotification">The type of the notification.</typeparam>
public interface IAsyncNotificationHandler<in TNotification>
    where TNotification : INotification
{
    /// <summary>
    /// Handles the specified notification asynchronously.
    /// </summary>
    /// <param name="notification">The notification.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task HandleAsync(TNotification notification, CancellationToken cancellationToken = default);
}