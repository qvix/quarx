namespace IQbx.Doser.Implementation.Lifetime
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    public static class ThreadScope
    {
        [ThreadStatic] private static ConcurrentDictionary<Guid, object> instances;

        private static ConcurrentDictionary<Guid, object> Instances =>
            LazyInitializer.EnsureInitialized(ref instances, () => new ConcurrentDictionary<Guid, object>());

        public static object Get(Guid key, Func<object> generator)
        {
            return Instances.GetOrAdd(key, generator);
        }

        public static void Dispose()
        {
            instances?.Clear();
        }
        
    }
}