namespace IQbx.Doser.Implementation
{
    using System;
    using System.Linq;

    internal class ObjectResolver  
    {
        private readonly IObjectResolver[] resolvers;

        public ObjectResolver(IObjectResolver[] resolvers)
        {
            this.resolvers = resolvers;
        }

        public object Get()
        {
            Func<object> result = new Func<object>(() => default);
            foreach (var resolver in resolvers.Reverse()) result = resolver.Resolve(result);
            return result();
        }
    }
}