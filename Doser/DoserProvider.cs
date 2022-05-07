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
            this.registrations.Add(registeredType, this.GetResolvers(new ObjectBuilder(implementationType, registrations), lifeTime));
        }

        public void Add(Type registeredType, Type implementationType, object key, InstanceLifetime lifeTime)
        {
            this.registrations.Add(registeredType, key, this.GetResolvers(new ObjectBuilder(implementationType, registrations), lifeTime));
        }

        public void Add<TInterface, TImplementation>(Func<TImplementation> factory, InstanceLifetime lifeTime) where TImplementation : TInterface
        {
            this.registrations.Add(typeof(TInterface), this.GetResolvers(new InstanceFactory(() => factory()), lifeTime));
        }

        public void Add<TInterface, TImplementation>(Func<TImplementation> factory, object key, InstanceLifetime lifeTime) where TImplementation : TInterface
        {
            this.registrations.Add(typeof(TInterface), key, this.GetResolvers(new InstanceFactory(() => factory()), lifeTime));
        }

        private IObjectResolver[] GetResolvers(IObjectResolver resolver, InstanceLifetime scope)
        {
            return scope switch
            {
                InstanceLifetime.Global => new [] { new SingletonLifetime(), resolver },
                InstanceLifetime.Local => new [] { resolver },
                InstanceLifetime.Scoped => new [] { new ScopeLifetime(this.scopeService), resolver },
                InstanceLifetime.ScopeTransparent => new [] { new ScopeTransparentLifetime(this.scopeService), resolver },
                _ => throw new Exception($"Unknown life time scope registeredType {scope}")
            };
        }
    }
}
