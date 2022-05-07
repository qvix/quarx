namespace Doser.Implementation
{
    using System;
    using System.Collections.Concurrent;

    internal class ResolverRepository : IDoserServiceProvider
    {
        private readonly ConcurrentDictionary<Type, TypeResolver> typeResolvers = new ();

        public bool TryGetValue(Type type, out TypeResolver result)
        {
            return this.typeResolvers.TryGetValue(type, out result);
        }

        public void Add(Type type, IObjectResolver[] resolvers)
        {
            this.Ensure(type).Add(resolvers);
        }

        public void Add(Type type, object key, IObjectResolver[] resolvers)
        {
            this.Ensure(type).Add(key, resolvers);
        }

        public object? GetService(Type serviceType, object key)
        {
            return this.Ensure(serviceType).GetResolver(key).Get();
        }

        public object? GetService(Type serviceType)
        {
            return this.Ensure(serviceType).GetResolver().Get();
        }

        public TypeResolver GetResolver(Type serviceType)
        {
            return this.Ensure(serviceType);
        }

        public IDoserServiceProvider Build()
        {
            foreach (var resolver in this.typeResolvers.Values)
            {
                resolver.Build();
            }

            return this;
        }


        private TypeResolver Ensure(Type type)
        {
            return this.typeResolvers.GetOrAdd(type, type => new TypeResolver(type, this));
        }
    }
}