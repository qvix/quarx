namespace Doser.Implementation.Lifetime
{
    using System;
    
    internal class ScopeLifetime: IObjectResolver
    {
        private readonly IScopeService scopeService;
        private readonly Guid key = Guid.NewGuid();

        public ScopeLifetime(IScopeService scopeService)
        {
            this.scopeService = scopeService;
        }

        public Func<object> Resolve(Func<object> next)
        {
            return () => this.scopeService.Get(key, next);
        }
    }
}