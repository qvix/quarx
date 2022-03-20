namespace Doser
{
    using System;
    using Implementation;
    using Implementation.Lifetime;

    public sealed class DoserProvider : IDoserServiceProvider
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

        public void Add(Type registeredType, Type implementationType, InstanceLifetime lifeTime)
        {
            this.registrations[registeredType].Add(this.GetResolvers(new ObjectBuilder(implementationType, registrations), lifeTime));
        }

        public void Add(Type registeredType, Type implementationType, object key, InstanceLifetime lifeTime)
        {
            this.registrations[registeredType].Add(key, this.GetResolvers(new ObjectBuilder(implementationType, registrations), lifeTime));
        }

        public void Add<TInterface, TImplementation>(Func<TImplementation> factory, InstanceLifetime lifeTime) where TImplementation : TInterface
        {
            this.registrations[typeof(TInterface)].Add(this.GetResolvers(new InstanceFactory(() => factory()), lifeTime));
        }

        public void Add<TInterface, TImplementation>(Func<TImplementation> factory, object key, InstanceLifetime lifeTime) where TImplementation : TInterface
        {
            this.registrations[typeof(TInterface)].Add(key, this.GetResolvers(new InstanceFactory(() => factory()), lifeTime));
        }

        public object? Get(Type type)
        {
            return this.registrations[type].GetResolver().Get();
        }

        public object? GetService(Type type, object key)
        {
            return this.registrations[type].GetResolver(key).Get();
        }

        public object GetService(Type serviceType) => Get(serviceType);

        private IObjectResolver[] GetResolvers(IObjectResolver resolver, InstanceLifetime scope)
        {
            return scope switch
            {
                InstanceLifetime.Global => new [] { new SingletonLifetime(), resolver },
                InstanceLifetime.Local => new [] { resolver },
                InstanceLifetime.Scoped => new [] { new ScopeLifetime(this.scopeService), resolver },
                _ => throw new Exception($"Unknown life time scope registeredType {scope}")
            };
        }
    }
}
