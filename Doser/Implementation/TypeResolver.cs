namespace Doser.Implementation
{
    using System;
    using System.Collections.Generic;
    
    using Generic;
    using Exceptions;

    internal class TypeResolver
    {
        private readonly Type type;
        private readonly List<ObjectResolver> registered = new();

        private IDictionary<object, ObjectResolver> keyResolvers;
        private readonly ResolverRepository typeResolvers;
        private ObjectResolver defaultResolver;

        public TypeResolver(Type type, ResolverRepository typeResolvers)
        {
            this.type = type;
            this.typeResolvers = typeResolvers;
        }

        public void Add(params IObjectResolver[] resolvers)
        {
            this.registered.Add(new ObjectResolver(resolvers));
        }

        public void Add(object key, params IObjectResolver[] resolvers)
        {
            key.CheckNotNull();

            this.keyResolvers ??= new Dictionary<object, ObjectResolver>();
            
            this.keyResolvers
                .Add(key, new ObjectResolver(resolvers));
        }

        public IEnumerable<ObjectResolver> GetResolvers()
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

        public void Build()
        {
            foreach (var resolver in this.GetResolvers())
            {
                resolver.Build();
            }

            this.defaultResolver = this.CreateResolver();
        }

        public ObjectResolver GetResolver()
        {
            return defaultResolver ??= this.CreateResolver();
        }

        public ObjectResolver GetResolver(object key)
        {
            key.CheckNotNull();
            if (this.keyResolvers == null)
            {
                return null;
            }

            if (!this.keyResolvers.TryGetValue(key, out var resolver))
            {
                resolver = FuncResolver.TryCreateFuncResolver(this.type, this.typeResolvers, key)
                           ?? LazyResolver.TryCreateLazyResolver(this.type, this.typeResolvers, key);

                this.keyResolvers[key] = resolver;
            }

            return resolver;

        }

        private ObjectResolver CreateResolver()
        {
            return this.registered.Count > 0
                ? this.registered[0]
                : EnumerableResolver.TryCreateEnumerableResolver(this.type, this.typeResolvers)
                  ?? FuncResolver.TryCreateFuncResolver(this.type, this.typeResolvers)
                  ?? LazyResolver.TryCreateLazyResolver(this.type, this.typeResolvers)
                  ?? this.TryCreateTypeResolver()
                  ?? throw new ResolveException(this.type);
        }

        private ObjectResolver TryCreateTypeResolver()
        {
            if (this.type.IsAbstract || this.type.IsInterface)
            {
                return null;
            }

            return new ObjectResolver(new ObjectBuilder(this.type, this.typeResolvers));
        }
    }
}