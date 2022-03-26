namespace Doser.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class ObjectResolver  
    {
        private static readonly Func<object> DefaultResult = new(() => default);
        private readonly IObjectResolver[] resolvers;

        public ObjectResolver(IEnumerable<IObjectResolver> resolvers)
        {
            this.resolvers = resolvers.Reverse().ToArray();
        }

        public object Get()
        {
            return resolvers.Aggregate(DefaultResult, (current, resolver) => resolver.Resolve(current))();
        }
    }
}