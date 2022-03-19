namespace IQbx.Doser.Implementation
{
    using System;

    internal class InstanceFactory : IObjectResolver
    {
        private readonly Func<object> instanceFactory;

        public InstanceFactory(Func<object> instanceFactory)
        {
            this.instanceFactory = instanceFactory;
        }

        public Func<object> Resolve(Func<object> next)
        {
            return instanceFactory;
        }
    }
}