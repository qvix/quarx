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

        public object Get()
        {
            return this.value ??= objectResolver.Get();
        }

        public void Build()
        {
            this.objectResolver.Build();
        }
    }
}