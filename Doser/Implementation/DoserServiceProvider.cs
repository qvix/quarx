using System;
using System.Collections.Concurrent;

namespace Doser.Implementation;

internal class DoserServiceProvider : IDoserServiceProvider
{
    private readonly ConcurrentDictionary<Type, Func<object>> registrations = new();
    private readonly ResolverRepository repository;

    public DoserServiceProvider(ResolverRepository repository)
    {
        this.repository = repository;
        //foreach(var item in repository.)
    }


    public Func<object?> GetResolver<T>()
    {
        return registrations.GetOrAdd(typeof(T), null!);
    }

    public object? GetService(Type serviceType, object key)
    {
        return registrations.TryGetValue(serviceType, out var result) ? result : null;
    }

    public object? GetService(Type serviceType)
    {
        return registrations.GetOrAdd(serviceType, null!)();
    }
}
