namespace Doser.Implementation.Lifetime
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    public static class Scope
    {
        private static AsyncLocal<ConcurrentDictionary<Guid, object>> instances =new AsyncLocal<ConcurrentDictionary<Guid, object>>();
        private static ConcurrentDictionary<Guid, object> Instances => instances.Value ??= new ConcurrentDictionary<Guid, object>();

        public static object Get(Guid key, Func<object> factory)
        {
            return Instances.GetOrAdd(key, factory);
        }

        public static void Dispose()
        {
            Instances.Clear();
        }        
    }
}