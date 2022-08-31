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
        return this.GetRequiredResolver(serviceType).GetResolver().Resolve();
    }

    public object? GetService(Type serviceType, object key)
    {
        return this.GetRequiredResolver(serviceType).GetResolver(key)?.Resolve();
    }

    public Func<object?> GetResolver<T>()
    {
        return this.GetRequiredResolver(typeof(T)).GetResolver;
    }

    public TypeResolver GetResolver(Type serviceType)
    {
        return this.EnsureTypeResolver(serviceType);
    }

    public IDoserServiceProvider Build()
    {
        foreach (var typeResolver in this.typeResolvers.Values)
        {
            typeResolver.Build();
        }

        return this;
    }

    private TypeResolver EnsureTypeResolver(Type type)
    {
        return this.typeResolvers.TryGetValue(type, out var resolver) 
            ? resolver 
            : this.typeResolvers.GetOrAdd(type, this.Create);
    }

    private TypeResolver GetRequiredResolver(Type type)
    {
        return this.typeResolvers.TryGetValue(type, out var resolver)
            ? resolver
            : this.typeResolvers.GetOrAdd(type, this.CreateAndBuild);
    }

    private TypeResolver Create(Type type)
    {
        return TypeResolver.Create(type, this);
    }
    
    private TypeResolver CreateAndBuild(Type type)
    {
        return TypeResolver.CreateAndBuild(type, this);
    }
}