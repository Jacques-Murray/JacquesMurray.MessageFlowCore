namespace JacquesMurray.MessageFlowCore;

using Abstractions;
using Abstractions.Messages;
using System.Reflection;

/// <summary>
/// Extension methods for the MessageFlowCore mediator.
/// </summary>
public static class MediatorExtensions
{
    /// <summary>
    /// Sends a request to the mediator and returns the response.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="mediator">The mediator instance.</param>
    /// <param name="query">The query to execute.</param>
    /// <returns>The result from the query handler.</returns>
    public static TResult Query<TResult>(this IMediator mediator, IQuery<TResult> query)
    {
        return mediator.Send(query);
    }

    /// <summary>
    /// Sends a request to the mediator asynchronously and returns the response.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="mediator">The mediator instance.</param>
    /// <param name="query">The query to execute.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response.</returns>
    public static Task<TResult> QueryAsync<TResult>(this IMediator mediator, IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        return mediator.SendAsync(query, cancellationToken);
    }

    /// <summary>
    /// Sends a command to the mediator.
    /// </summary>
    /// <param name="mediator">The mediator instance.</param>
    /// <param name="command">The command to execute.</param>
    public static void Execute(this IMediator mediator, ICommand command)
    {
        mediator.Send(command);
    }

    /// <summary>
    /// Sends a command to the mediator asynchronously.
    /// </summary>
    /// <param name="mediator">The mediator instance.</param>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static Task ExecuteAsync(this IMediator mediator, ICommand command, CancellationToken cancellationToken = default)
    {
        return mediator.SendAsync(command, cancellationToken);
    }

    /// <summary>
    /// Publishes an event to the mediator.
    /// </summary>
    /// <param name="mediator">The mediator instance.</param>
    /// <param name="event">The event to publish.</param>
    public static void Raise(this IMediator mediator, IEvent @event)
    {
        mediator.Publish(@event);
    }

    /// <summary>
    /// Publishes an event to the mediator asynchronously.
    /// </summary>
    /// <param name="mediator">The mediator instance.</param>
    /// <param name="event">The event to publish.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static Task RaiseAsync(this IMediator mediator, IEvent @event, CancellationToken cancellationToken = default)
    {
        return mediator.PublishAsync(@event, cancellationToken);
    }
}