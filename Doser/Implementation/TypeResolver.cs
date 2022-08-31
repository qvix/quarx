namespace Doser.Implementation;

using System;
using System.Collections.Generic;
using System.Linq;

using Generic;
using Exceptions;

internal class TypeResolver
{
    private readonly List<IObjectResolver> registered = new();
    private IDictionary<object, IObjectResolver>? keyResolvers;
    private readonly ResolverRepository repository;
    private IObjectResolver resolver = default!;
    private IObjectResolver[] resolvers = default!;

    public static TypeResolver Create(Type type, ResolverRepository repository)
    {
        return new TypeResolver(type, repository);
    }

    public static TypeResolver CreateAndBuild(Type type, ResolverRepository repository)
    {
        var resolver = new TypeResolver(type, repository);

        resolver.Build();

        return resolver;
    }

    private TypeResolver(Type type, ResolverRepository repository)
    {
        this.Type = type;
        this.repository = repository;
    }

    public Type Type { get; }

    public void Add(IObjectResolver objectResolver)
    {
        this.registered.Add(objectResolver);
    }

    public void Add(object key, IObjectResolver objectResolver)
    {
        this.keyResolvers ??= new Dictionary<object, IObjectResolver>();
            
        this.keyResolvers.Add(key, objectResolver);
    }

    public void Build()
    {
        this.resolvers = this.GetResolversInternal().ToArray();

        foreach (var objectResolver in this.resolvers)
        {
            objectResolver.Build();
        }

        this.resolver = this.registered.Count > 0 
            ? this.registered.Last()
            : this.CreateResolver()?.Build()!;
    }

    public IObjectResolver GetResolver()
    {
        return this.resolver;
    }

    public IObjectResolver? GetResolver(object key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (this.keyResolvers == null)
        {
            return null;
        }

        if (!this.keyResolvers.TryGetValue(key, out var keyResolver))
        {
            keyResolver = FuncResolver.TryCreateFuncResolver(this.Type, this.repository, key)
                       ?? LazyResolver.TryCreateLazyResolver(this.Type, this.repository, key)
                       ?? throw new ResolveException(this.Type, key); 

            this.keyResolvers[key] = keyResolver.Build();
        }

        return keyResolver;
    }

    public IObjectResolver[] GetResolvers()
    {
        return this.resolvers;
    }

    private IEnumerable<IObjectResolver> GetResolversInternal()
    {
        foreach (var registeredResolver in this.registered)
        {
            yield return registeredResolver;
        }

        if (this.keyResolvers == null)
        {
            yield break;
        }

        foreach (var keyResolver in this.keyResolvers)
        {
            yield return keyResolver.Value;
        }
    }

    private IObjectResolver? CreateResolver()
    {
        return this.registered.Count > 0
            ? this.registered[0]
            : EnumerableResolver.TryCreateEnumerableResolver(this.Type, this.repository)
              ?? FuncResolver.TryCreateFuncResolver(this.Type, this.repository)
              ?? LazyResolver.TryCreateLazyResolver(this.Type, this.repository)
              ?? this.TryCreateTypeResolver()
              ?? (this.resolvers.Any() ? null : throw new ResolveException(this.Type));
    }

    private IObjectResolver? TryCreateTypeResolver()
    {
        if (this.Type.IsAbstract || this.Type.IsInterface)
        {
            return null;
        }

        return new ObjectBuilder(this.Type, this.repository);
    }
}