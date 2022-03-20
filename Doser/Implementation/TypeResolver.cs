namespace Doser.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    
    using Generic;
    using Exceptions;

    internal class TypeResolver
    {
        private readonly Type type;
        private readonly ICollection<ObjectResolver> registered = new List<ObjectResolver>(1);

        private IDictionary<object, ObjectResolver> keyResolvers;
        private readonly ResolverRepository typeResolvers;

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

            LazyInitializer
                .EnsureInitialized(ref this.keyResolvers, () => new Dictionary<object, ObjectResolver>())
                .Add(key, new ObjectResolver(resolvers));
        }

        public IEnumerable<ObjectResolver> GetResolvers()
        {
            foreach (var policy in this.registered)
            {
                yield return policy;
            }

            if (this.keyResolvers == null)
            {
                yield break;
            }

            foreach (var policy in this.keyResolvers)
            {
                yield return policy.Value;
            }
        }

        public ObjectResolver GetResolver()
        {
            return this.registered.FirstOrDefault()
                   ?? EnumerableResolver.TryCreateEnumerableResolver(this.type, this.typeResolvers)
                   ?? FuncResolver.TryCreateFuncResolver(this.type, this.typeResolvers)
                   ?? LazyResolver.TryCreateLazyResolver(this.type, this.typeResolvers)
                   ?? this.TryCreateTypeResolver()
                   ?? throw new ResolveException(this.type);
        }

        public ObjectResolver GetResolver(object key)
        {
            key.CheckNotNull();
            if (this.keyResolvers == null)
            {
                throw new ResolveException(this.type);
            }
            return this.keyResolvers.TryGetValue(key, out var resolver) ? resolver :
                 FuncResolver.TryCreateFuncResolver(this.type, this.typeResolvers, key)
                 ?? LazyResolver.TryCreateLazyResolver(this.type, this.typeResolvers)
                 ?? throw new ResolveException(this.type);
        }

        private ObjectResolver TryCreateTypeResolver()
        {
            if (this.type.IsAbstract || this.type.IsInterface)
            {
                return null;
            }

            return new ObjectResolver(new IObjectResolver[] { new ObjectBuilder(this.type, this.typeResolvers) });
        }
    }
}