namespace JacquesMurray.MessageFlowCore.Abstractions.Messages;

/// <summary>
/// Marker interface for all messages that can be dispatched through the mediator.
/// </summary>
public interface IMessage
{
}

/// <summary>
/// Represents a request message that expects a response of type TResponse.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IRequest<out TResponse> : IMessage
{
}

/// <summary>
/// Represents a command message that doesn't return a result.
/// </summary>
public interface ICommand : IRequest<Unit>
{
}

/// <summary>
/// Represents a query message that returns a result of type TResult.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface IQuery<out TResult> : IRequest<TResult>
{
}

/// <summary>
/// Represents a notification message that will be dispatched to multiple handlers.
/// </summary>
public interface INotification : IMessage
{
}

/// <summary>
/// Represents an event message that will be dispatched to multiple handlers.
/// </summary>
public interface IEvent : INotification
{
}