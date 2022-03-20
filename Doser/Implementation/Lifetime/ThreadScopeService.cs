namespace Doser.Implementation.Lifetime
{
    using System;

    public class ThreadScopeService : IScopeService
    {
        public object Get(Guid key, Func<object> next)
        {
            return Scope.Get(key, next);
        }

        public void Close()
        {
            Scope.Dispose();
        }
    }
}