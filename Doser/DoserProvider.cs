namespace Doser;

using System;
using Implementation;
using Implementation.Lifetime;

public sealed class DoserProvider 
{
    private readonly ResolverRepository registrations = new();
    private readonly IScopeService scopeService;

    public DoserProvider():this(new ThreadScopeService())
    {
    }

    public DoserProvider(IScopeService scopeService)
    {
        ArgumentNullException.ThrowIfNull(scopeService);
            
        this.scopeService = scopeService;

        this.Add<IScopeService, IScopeService>(() => scopeService, InstanceLifetime.Global);
        this.Add<IServiceProvider, IServiceProvider>(() => this.registrations, InstanceLifetime.Global);
        this.Add<IDoserServiceProvider, IDoserServiceProvider>(() => this.registrations, InstanceLifetime.Global);
    }

    public IDoserServiceProvider Build()
    {
        return this.registrations.Build();
    }

    public DoserProvider Add(Type registeredType, Type implementationType, InstanceLifetime lifeTime)
    {
        ArgumentNullException.ThrowIfNull(registeredType);
        ArgumentNullException.ThrowIfNull(implementationType);
        ArgumentNullException.ThrowIfNull(lifeTime);

        this.registrations.Add(registeredType, this.GetResolver(new ObjectBuilder(implementationType, this.registrations), lifeTime));
        return this;
    }

    public DoserProvider Add(Type registeredType, Type implementationType, object key, InstanceLifetime lifeTime)
    {
        ArgumentNullException.ThrowIfNull(registeredType);
        ArgumentNullException.ThrowIfNull(implementationType);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(lifeTime);

        this.registrations.Add(registeredType, key, this.GetResolver(new ObjectBuilder(implementationType, this.registrations), lifeTime));
        return this;
    }

    public DoserProvider Add<TInterface, TImplementation>(Func<TImplementation> factory, InstanceLifetime lifeTime) where TImplementation : TInterface
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(lifeTime);

        this.registrations.Add(typeof(TInterface), this.GetResolver(new InstanceFactory(() => factory(), lifeTime), lifeTime));
        return this;
    }

    public DoserProvider Add<TInterface, TImplementation>(Func<TImplementation> factory, object key, InstanceLifetime lifeTime) where TImplementation : TInterface
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(lifeTime);

        this.registrations.Add(typeof(TInterface), key, this.GetResolver(new InstanceFactory(() => factory(), lifeTime), lifeTime));
        return this;
    }

    private IObjectResolver GetResolver(IObjectResolver resolver, InstanceLifetime scope)
    {
        return scope switch
        {
            InstanceLifetime.Global => new SingletonLifetime(resolver),
            InstanceLifetime.Local => resolver,
            InstanceLifetime.Scoped => new ScopeLifetime(this.scopeService, resolver),
            InstanceLifetime.ScopeTransparent => new ScopeTransparentLifetime(this.scopeService, resolver),
            _ => throw new Exception($"Unknown life time scope registeredType {scope}")
        };
    }
}