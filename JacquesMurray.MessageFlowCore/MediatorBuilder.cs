namespace JacquesMurray.MessageFlowCore;

using Abstractions;
using Abstractions.Handlers;
using Abstractions.Messages;
using System.Reflection;

/// <summary>
/// Builder class for configuring and creating an instance of the <see cref="IMediator"/>.
/// </summary>
public sealed class MediatorBuilder
{
    private readonly ServiceContainer _serviceContainer = new();

    /// <summary>
    /// Registers a handler for a specific message type.
    /// </summary>
    /// <typeparam name="THandler">The handler type.</typeparam>
    /// <param name="handler">The handler instance.</param>
    /// <returns>The builder for method chaining.</returns>
    public MediatorBuilder RegisterHandler<THandler>(THandler handler)
    {
        RegisterHandlerInstance(handler);
        return this;
    }

    /// <summary>
    /// Registers a factory function for creating a handler.
    /// </summary>
    /// <typeparam name="THandler">The handler type.</typeparam>
    /// <param name="factory">The factory function.</param>
    /// <returns>The builder for method chaining.</returns>
    public MediatorBuilder RegisterHandler<THandler>(Func<THandler> factory)
    {
        RegisterHandlerFactory(factory);
        return this;
    }

    /// <summary>
    /// Registers all handlers from the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to scan for handlers.</param>
    /// <returns>The builder for method chaining.</returns>
    public MediatorBuilder RegisterHandlers(Assembly assembly)
    {
        ScanAndRegisterHandlers(assembly);
        return this;
    }

    /// <summary>
    /// Registers all handlers from the specified assemblies.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan for handlers.</param>
    /// <returns>The builder for method chaining.</returns>
    public MediatorBuilder RegisterHandlers(params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            ScanAndRegisterHandlers(assembly);
        }
        return this;
    }

    /// <summary>
    /// Registers all handlers from the assembly containing the specified type.
    /// </summary>
    /// <typeparam name="T">The type whose assembly to scan for handlers.</typeparam>
    /// <returns>The builder for method chaining.</returns>
    public MediatorBuilder RegisterHandlersFromAssemblyContaining<T>()
    {
        ScanAndRegisterHandlers(typeof(T).Assembly);
        return this;
    }

    /// <summary>
    /// Builds and returns the configured mediator instance.
    /// </summary>
    /// <returns>An instance of <see cref="IMediator"/>.</returns>
    public IMediator Build()
    {
        return new Mediator(_serviceContainer);
    }

    #region Private Helper Methods

    private void RegisterHandlerInstance<THandler>(THandler handler)
    {
        var handlerType = typeof(THandler);
        RegisterHandlerInterfaces(handlerType, () => handler ?? throw new InvalidOperationException("Handler instance is null."));
    }

    private void RegisterHandlerFactory<THandler>(Func<THandler> factory)
    {
        var handlerType = typeof(THandler);
        RegisterHandlerInterfaces(handlerType, () => factory() ?? throw new InvalidOperationException("Factory returned null."));
    }

    private void ScanAndRegisterHandlers(Assembly assembly)
    {
        // Find all concrete (non-abstract, non-interface) types
        var concreteTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsInterface);

        // Register each concrete type that implements any handler interface
        foreach (var handlerType in concreteTypes)
        {
            var interfaces = handlerType.GetInterfaces();
            var hasHandlerInterface = interfaces.Any(i =>
                (i.IsGenericType && (
                    i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
                    i.GetGenericTypeDefinition() == typeof(IAsyncRequestHandler<,>) ||
                    i.GetGenericTypeDefinition() == typeof(INotificationHandler<>) ||
                    i.GetGenericTypeDefinition() == typeof(IAsyncNotificationHandler<>)
                ))
            );

            if (hasHandlerInterface)
            {
                // Create a factory to instantiate the handler when needed
                var handlerFactory = CreateHandlerFactory(handlerType);
                RegisterHandlerInterfaces(handlerType, handlerFactory);
            }
        }
    }

    private Func<object> CreateHandlerFactory(Type handlerType)
    {
        // Simple factory that uses default constructor
        // In a real application, this might use dependency injection
        return () => Activator.CreateInstance(handlerType) ?? throw new InvalidOperationException($"Unable to create instance of type {handlerType.FullName}.");
    }

    private void RegisterHandlerInterfaces(Type handlerType, Func<object> handlerFactory)
    {
        // Register all request handler interfaces
        RegisterRequestHandlerInterfaces(handlerType, handlerFactory, typeof(IRequestHandler<,>));
        RegisterRequestHandlerInterfaces(handlerType, handlerFactory, typeof(IAsyncRequestHandler<,>));

        // Register all notification handler interfaces
        RegisterNotificationHandlerInterfaces(handlerType, handlerFactory, typeof(INotificationHandler<>));
        RegisterNotificationHandlerInterfaces(handlerType, handlerFactory, typeof(IAsyncNotificationHandler<>));
    }

    private void RegisterRequestHandlerInterfaces(Type handlerType, Func<object> handlerFactory, Type handlerInterfaceType)
    {
        foreach (var interfaceType in handlerType.GetInterfaces())
        {
            if (!interfaceType.IsGenericType) continue;

            var genericTypeDefinition = interfaceType.GetGenericTypeDefinition();
            if (genericTypeDefinition != handlerInterfaceType) continue;

            // Register the concrete handler against its exact interface type
            _serviceContainer.Register(interfaceType, handlerFactory);
        }
    }

    private void RegisterNotificationHandlerInterfaces(Type handlerType, Func<object> handlerFactory, Type handlerInterfaceType)
    {
        foreach (var interfaceType in handlerType.GetInterfaces())
        {
            if (!interfaceType.IsGenericType) continue;

            var genericTypeDefinition = interfaceType.GetGenericTypeDefinition();
            if (genericTypeDefinition != handlerInterfaceType) continue;

            // For notification handlers, we want to register the same handler for all notification types it can handle
            _serviceContainer.Register(interfaceType, handlerFactory);
        }
    }

    #endregion
}

/// <summary>
/// Extensions methods for creating a mediator using the builder pattern.
/// </summary>
public static class MediatorBuilderExtensions
{
    /// <summary>
    /// Creates a new instance of the <see cref="MediatorBuilder"/>.
    /// </summary>
    /// <returns>A new mediator builder.</returns>
    public static MediatorBuilder CreateMediator()
    {
        return new MediatorBuilder();
    }
}