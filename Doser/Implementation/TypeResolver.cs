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
    private IObjectResolver? defaultResolver;
    private IObjectResolver[]? defaultResolvers;

    public TypeResolver(Type type, ResolverRepository repository)
    {
        this.Type = type;
        this.repository = repository;
    }

    public Type Type { get; }

    public void Add(IObjectResolver resolver)
    {
        this.registered.Add(resolver);
    }

    public void Add(object key, IObjectResolver resolver)
    {
        this.keyResolvers ??= new Dictionary<object, IObjectResolver>();
            
        this.keyResolvers.Add(key, resolver);
    }

    public void Build()
    {
        this.defaultResolvers = this.GetResolversInternal().ToArray();

        foreach (var resolver in this.defaultResolvers)
        {
            resolver.Build();
        }

        this.defaultResolver = this.registered.Count > 0 
            ? this.registered.Last() 
            : null;
    }

    public IObjectResolver GetResolver()
    {
        return this.defaultResolver ??= this.CreateResolver();
    }

    public IObjectResolver? GetResolver(object key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (this.keyResolvers == null)
        {
            return null;
        }

        if (!this.keyResolvers.TryGetValue(key, out var resolver))
        {
            resolver = FuncResolver.TryCreateFuncResolver(this.Type, this.repository, key)
                       ?? LazyResolver.TryCreateLazyResolver(this.Type, this.repository, key)
                       ?? throw new ResolveException(this.Type, key); 

            this.keyResolvers[key] = resolver;
        }

        return resolver;
    }

    public IObjectResolver[] GetResolvers()
    {
        return this.defaultResolvers!;
    }

    private IEnumerable<IObjectResolver> GetResolversInternal()
    {
        foreach (var resolver in this.registered)
        {
            yield return resolver;
        }

        if (this.keyResolvers == null)
        {
            yield break;
        }

        foreach (var resolver in this.keyResolvers)
        {
            yield return resolver.Value;
        }
    }

    private IObjectResolver CreateResolver()
    {
        return this.registered.Count > 0
            ? this.registered[0]
            : EnumerableResolver.TryCreateEnumerableResolver(this.Type, this.repository)
              ?? FuncResolver.TryCreateFuncResolver(this.Type, this.repository)
              ?? LazyResolver.TryCreateLazyResolver(this.Type, this.repository)
              ?? this.TryCreateTypeResolver()
              ?? throw new ResolveException(this.Type);
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