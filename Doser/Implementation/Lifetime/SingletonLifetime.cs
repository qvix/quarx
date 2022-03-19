namespace IQbx.Doser.Implementation.Lifetime
{
    using System;

    internal class SingletonLifetime : IObjectResolver
    {
        private object value;

        public Func<object> Resolve(Func<object> next)
        {
            return () => value ??= next();
        }
    }
}