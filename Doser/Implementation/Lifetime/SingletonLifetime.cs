using System;

namespace Doser.Implementation.Lifetime
{
    internal class SingletonLifetime : IObjectResolver
    {
        private readonly IObjectResolver objectResolver;
        private object value;

        public SingletonLifetime(IObjectResolver objectResolver)
        {
            this.objectResolver = objectResolver;
        }

        public InstanceLifetime Lifetime => InstanceLifetime.Global;

        public Func<object> GetResolver()
        {
            return () => this.value ??= objectResolver.GetResolver()();
        }

        public void Build()
        {
            this.objectResolver.Build();
        }
    }
}