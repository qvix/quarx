using System.Linq;

namespace Doser.Implementation
{
    using System;
    using System.Collections.Generic;
    
    using Generic;
    using Exceptions;

    internal class TypeResolver
    {
        private readonly Type type;
        private readonly List<IObjectResolver> registered = new();

        private IDictionary<object, IObjectResolver> keyResolvers;
        private readonly ResolverRepository typeResolvers;
        private IObjectResolver defaultResolver;
        private IObjectResolver[] defaultResolvers;

        public TypeResolver(Type type, ResolverRepository typeResolvers)
        {
            this.type = type;
            this.typeResolvers = typeResolvers;
        }

        public void Add(IObjectResolver resolver)
        {
            this.registered.Add(resolver);
        }

        public void Add(object key, IObjectResolver resolver)
        {
            key.CheckNotNull();

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

            this.defaultResolver = this.defaultResolvers.First();
        }

        public IObjectResolver GetResolver()
        {
            return defaultResolver ??= this.CreateResolver();
        }

        public IObjectResolver GetResolver(object key)
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

        public IObjectResolver[] GetResolvers()
        {
            return this.defaultResolvers;
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
                : EnumerableResolver.TryCreateEnumerableResolver(this.type, this.typeResolvers)
                  ?? FuncResolver.TryCreateFuncResolver(this.type, this.typeResolvers)
                  ?? LazyResolver.TryCreateLazyResolver(this.type, this.typeResolvers)
                  ?? this.TryCreateTypeResolver()
                  ?? throw new ResolveException(this.type);
        }

        private IObjectResolver TryCreateTypeResolver()
        {
            if (this.type.IsAbstract || this.type.IsInterface)
            {
                return null;
            }

            return new ObjectBuilder(this.type, this.typeResolvers);
        }
    }
}