namespace Doser.Implementation.Lifetime
{
    using System;
    
    internal class ScopeTransparentLifetime : IObjectResolver
    {
        private readonly IScopeService scopeService;
        private readonly Guid key = Guid.NewGuid();

        public ScopeTransparentLifetime(IScopeService scopeService)
        {
            this.scopeService = scopeService;
        }

        public Func<object> Resolve(Func<object> next)
        {
            return () => this.scopeService.Current?.GetTransparent(key, next);
        }
    }
}