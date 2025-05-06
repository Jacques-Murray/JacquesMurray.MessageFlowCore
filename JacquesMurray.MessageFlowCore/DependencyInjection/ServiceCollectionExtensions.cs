namespace JacquesMurray.MessageFlowCore.DependencyInjection;

using Abstractions;
using Abstractions.Handlers;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

/// <summary>
/// Extension methods for registering MessageFlowCore services with Microsoft.Extensions.DependencyInjection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the MessageFlowCore mediator to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMessageFlow(this IServiceCollection services)
    {
        // Register the internal service container that will be used by the mediator
        services.AddSingleton<ServiceContainer>();

        // Register the mediator as a singleton
        services.AddSingleton<IMediator, Mediator>();

        return services;
    }

    /// <summary>
    /// Adds the MessageFlowCore mediator to the service collection and registers handlers from the specified assemblies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">The assemblies to scan for handlers.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMessageFlow(this IServiceCollection services, params Assembly[] assemblies)
    {
        // Add the basic MessageFlow services
        services.AddMessageFlow();

        // Register all handlers from the specified assemblies
        foreach (var assembly in assemblies)
        {
            RegisterHandlersFromAssembly(services, assembly);
        }

        // Register a factory that creates handlers using the service provider
        services.AddSingleton<IMessageFlowServiceResolver>(sp => new MessageFlowServiceResolver(sp));

        return services;
    }

    /// <summary>
    /// Adds the MessageFlowCore mediator to the service collection and registers handlers from the assembly containing the specified type.
    /// </summary>
    /// <typeparam name="T">A type in the assembly to scan for handlers.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMessageFlowFromAssemblyContaining<T>(this IServiceCollection services)
    {
        return services.AddMessageFlow(typeof(T).Assembly);
    }

    private static void RegisterHandlersFromAssembly(IServiceCollection services, Assembly assembly)
    {
        // Find all handler implementations in the assembly
        var handlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType);

        foreach (var handlerType in handlerTypes)
        {
            RegisterHandlerType(services, handlerType);
        }
    }

    private static void RegisterHandlerType(IServiceCollection services, Type handlerType)
    {
        // Register request handlers
        RegisterGenericInterfaceHandler(services, handlerType, typeof(IRequestHandler<,>));
        RegisterGenericInterfaceHandler(services, handlerType, typeof(IAsyncRequestHandler<,>));

        // Register notification handlers
        RegisterGenericInterfaceHandler(services, handlerType, typeof(INotificationHandler<>));
        RegisterGenericInterfaceHandler(services, handlerType, typeof(IAsyncNotificationHandler<>));
    }

    private static void RegisterGenericInterfaceHandler(IServiceCollection services, Type handlerType, Type handlerInterfaceType)
    {
        foreach (var implementedInterface in handlerType.GetInterfaces())
        {
            if (!implementedInterface.IsGenericType) continue;

            var genericTypeDefinition = implementedInterface.GetGenericTypeDefinition();
            if (genericTypeDefinition == handlerInterfaceType)
            {
                // Register the concrete handler type against the interface
                services.AddTransient(implementedInterface, handlerType);
            }
        }
    }
}

/// <summary>
/// Interface for resolving services within MessageFlowCore.
/// </summary>
public interface IMessageFlowServiceResolver
{
    /// <summary>
    /// Resolves a service of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of service to resolve.</typeparam>
    /// <returns>The resolved service.</returns>
    T? GetService<T>() where T : class;

    /// <summary>
    /// Resolves a service of the specified type.
    /// </summary>
    /// <param name="serviceType">The type of service to resolve.</param>
    /// <returns>The resolved service.</returns>
    object? GetService(Type serviceType);

    /// <summary>
    /// Resolves multiple services of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of services to resolve.</typeparam>
    /// <returns>The resolved services.</returns>
    IEnumerable<T> GetServices<T>() where T : class;

    /// <summary>
    /// Resolves multiple services of the specified type.
    /// </summary>
    /// <param name="serviceType">The type of services to resolve.</param>
    /// <returns>The resolved services.</returns>
    IEnumerable<object> GetServices(Type serviceType);
}

/// <summary>
/// Implementation of <see cref="IMessageFlowServiceResolver"/> that uses the .NET dependency injection container.
/// </summary>
internal class MessageFlowServiceResolver : IMessageFlowServiceResolver
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageFlowServiceResolver"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public MessageFlowServiceResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc />
    public T? GetService<T>() where T : class
    {
        return _serviceProvider.GetService(typeof(T)) as T;
    }

    /// <inheritdoc />
    public object? GetService(Type serviceType)
    {
        return _serviceProvider.GetService(serviceType);
    }

    /// <inheritdoc />
    public IEnumerable<T> GetServices<T>() where T : class
    {
        return _serviceProvider.GetServices(typeof(T)).Cast<T?>().Where(x => x != null)!;
    }

    /// <inheritdoc />
    public IEnumerable<object> GetServices(Type serviceType)
    {
        // Fix nullability warning by explicitly converting to non-nullable objects
        return _serviceProvider.GetServices(serviceType).OfType<object>();
    }
}