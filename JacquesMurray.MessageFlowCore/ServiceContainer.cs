namespace JacquesMurray.MessageFlowCore;

/// <summary>
/// A simple service container for resolving handlers.
/// </summary>
public class ServiceContainer
{
    private readonly Dictionary<Type, Func<object>> _registrations = new();

    /// <summary>
    /// Registers a factory function for a service type.
    /// </summary>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <param name="factory">The factory function that creates an instance of the service.</param>
    public void Register<TService>(Func<object> factory)
    {
        _registrations[typeof(TService)] = factory;
    }

    /// <summary>
    /// Registers a concrete instance for a service type.
    /// </summary>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <param name="instance">The instance to register.</param>
    public void Register<TService>(object instance)
    {
        _registrations[typeof(TService)] = () => instance;
    }

    /// <summary>
    /// Registers a factory function for a specific service type.
    /// </summary>
    /// <param name="serviceType">The service type.</param>
    /// <param name="factory">The factory function.</param>
    public void Register(Type serviceType, Func<object> factory)
    {
        _registrations[serviceType] = factory;
    }

    /// <summary>
    /// Registers a concrete instance for a specific service type.
    /// </summary>
    /// <param name="serviceType">The service type.</param>
    /// <param name="instance">The instance to register.</param>
    public void Register(Type serviceType, object instance)
    {
        _registrations[serviceType] = () => instance;
    }

    /// <summary>
    /// Resolves a service instance by type.
    /// </summary>
    /// <typeparam name="TService">The service type to resolve.</typeparam>
    /// <returns>The resolved service instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the service type is not registered.</exception>
    public TService? Resolve<TService>()
    {
        return (TService?)Resolve(typeof(TService));
    }

    /// <summary>
    /// Resolves a service instance by type.
    /// </summary>
    /// <param name="serviceType">The service type to resolve.</param>
    /// <returns>The resolved service instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the service type is not registered.</exception>
    public object? Resolve(Type serviceType)
    {
        // Check for exact match first
        if (_registrations.TryGetValue(serviceType, out var factory))
        {
            return factory();
        }

        // Look for open generic registrations
        if (serviceType.IsGenericType)
        {
            var openGenericType = serviceType.GetGenericTypeDefinition();

            // Find all matching open generic registrations
            var matchingRegistrations = _registrations
                .Where(r => r.Key.IsGenericType && r.Key.GetGenericTypeDefinition() == openGenericType)
                .ToList();

            if (matchingRegistrations.Any())
            {
                // If multiple registrations exist, you might want to implement a strategy to pick the right one
                // For example, you could prioritize registrations based on specific type constraints
                var bestMatch = matchingRegistrations.First().Value; // Simple strategy: pick the first one
                return bestMatch();
            }

            // Check directly for the open generic type
            if (_registrations.TryGetValue(openGenericType, out var openGenericFactory))
            {
                return openGenericFactory();
            }
        }

        throw new InvalidOperationException($"Service {serviceType.Name} is not registered.");
    }

    /// <summary>
    /// Resolves all services of a specific type.
    /// </summary>
    /// <typeparam name="TService">The service type to resolve.</typeparam>
    /// <returns>An enumerable of resolved service instances.</returns>
    public IEnumerable<TService> ResolveAll<TService>()
    {
        var serviceType = typeof(TService);
        return ResolveAll(serviceType).Cast<TService>();
    }

    /// <summary>
    /// Resolves all services of a specific type.
    /// </summary>
    /// <param name="serviceType">The service type to resolve.</param>
    /// <returns>An enumerable of resolved service instances.</returns>
    public IEnumerable<object> ResolveAll(Type serviceType)
    {
        var result = new List<object>();

        // Check for exact matches
        if (_registrations.TryGetValue(serviceType, out var factory))
        {
            var instance = factory();
            if (instance != null)
            {
                result.Add(instance);
            }
        }

        // Check for assignable types (for covariance/contravariance)
        foreach (var registration in _registrations)
        {
            // Skip exact matches (already handled)
            if (registration.Key == serviceType)
            {
                continue;
            }

            // Check if the registration is assignable to the requested service type
            if (serviceType.IsAssignableFrom(registration.Key))
            {
                var instance = registration.Value();
                if (instance != null)
                {
                    result.Add(instance);
                }
            }

            // Handle generic interface implementations
            if (serviceType.IsGenericType && registration.Key.IsGenericType)
            {
                var openServiceType = serviceType.GetGenericTypeDefinition();
                var openRegistrationType = registration.Key.GetGenericTypeDefinition();

                if (openServiceType == openRegistrationType)
                {
                    var instance = registration.Value();
                    if (instance != null)
                    {
                        result.Add(instance);
                    }
                }
            }
        }

        return result;
    }
}