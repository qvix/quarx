namespace Doser.Implementation
{
    using System;
    using System.Collections.Concurrent;

    internal class ResolverRepository
    {
        private readonly ConcurrentDictionary<Type, TypeResolver> typeResolvers = new ConcurrentDictionary<Type, TypeResolver>();

        public TypeResolver this[Type type] => this.Ensure(type);

        public bool TryGetValue(Type type, out TypeResolver result)
        {
            return this.typeResolvers.TryGetValue(type, out result);
        }

        private TypeResolver Ensure(Type type)
        {
            return this.typeResolvers.GetOrAdd(type, new TypeResolver(type, this));
        }
    }
}