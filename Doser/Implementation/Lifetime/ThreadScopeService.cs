namespace IQbx.Doser.Implementation.Lifetime
{
    using System;

    public class ThreadScopeService : IScopeService
    {
        public object Get(Guid key, Func<object> next)
        {
            return ThreadScope.Get(key, next);
        }

        public void Close()
        {
            ThreadScope.Dispose();
        }
    }
}