namespace Doser.Implementation;

using System;
using System.Collections.Concurrent;

internal class ResolverRepository : IDoserServiceProvider
{
    private readonly ConcurrentDictionary<Type, TypeResolver> typeResolvers = new ();
        
    public bool TryGetValue(Type type, out TypeResolver? result)
    {
        return this.typeResolvers.TryGetValue(type, out result);
    }

    public void Add(Type type, IObjectResolver resolver)
    {
        this.EnsureTypeResolver(type).Add(resolver);
    }

    public void Add(Type type, object key, IObjectResolver resolver)
    {
        this.EnsureTypeResolver(type).Add(key, resolver);
    }

    public object? GetService(Type serviceType)
    {
        return this.EnsureTypeResolver(serviceType).GetResolver().Resolve();
    }

    public object? GetService(Type serviceType, object key)
    {
        return this.EnsureTypeResolver(serviceType).GetResolver(key)?.Resolve();
    }

    public Func<object> GetResolver<T>()
    {
        return this.EnsureTypeResolver(typeof(T)).GetResolver().Resolve!;
    }

    public TypeResolver GetResolver(Type serviceType)
    {
        return this.EnsureTypeResolver(serviceType);
    }

    public IDoserServiceProvider Build()
    {
        foreach (var resolver in this.typeResolvers.Values)
        {
            resolver.Build();
        }

        return this;
    }

    private TypeResolver EnsureTypeResolver(Type type)
    {
        return this.typeResolvers.TryGetValue(type, out var resolver) 
            ? resolver 
            : this.typeResolvers.GetOrAdd(type, _ => new TypeResolver(type, this));
    }
}