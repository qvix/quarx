namespace Doser
{
    using System;
    using Implementation;
    using Implementation.Lifetime;

    public sealed class DoserProvider 
    {
        private readonly ResolverRepository registrations = new();
        private readonly IScopeService scopeService;

        public DoserProvider()
        {
            this.scopeService = new ThreadScopeService();
        }

        public DoserProvider(IScopeService scopeService)
        {
            this.scopeService = scopeService;
        }

        public IDoserServiceProvider Build()
        {
            return this.registrations.Build();
        }

        public void Add(Type registeredType, Type implementationType, InstanceLifetime lifeTime)
        {
            this.registrations.Add(registeredType, this.GetResolver(new ObjectBuilder(implementationType, registrations), lifeTime));
        }

        public void Add(Type registeredType, Type implementationType, object key, InstanceLifetime lifeTime)
        {
            this.registrations.Add(registeredType, key, this.GetResolver(new ObjectBuilder(implementationType, registrations), lifeTime));
        }

        public void Add<TInterface, TImplementation>(Func<TImplementation> factory, InstanceLifetime lifeTime) where TImplementation : TInterface
        {
            this.registrations.Add(typeof(TInterface), this.GetResolver(new InstanceFactory(() => factory(), lifeTime), lifeTime));
        }

        public void Add<TInterface, TImplementation>(Func<TImplementation> factory, object key, InstanceLifetime lifeTime) where TImplementation : TInterface
        {
            this.registrations.Add(typeof(TInterface), key, this.GetResolver(new InstanceFactory(() => factory(), lifeTime), lifeTime));
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
}
