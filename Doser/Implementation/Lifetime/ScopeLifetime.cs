﻿namespace Doser.Implementation.Lifetime
{
    using System;
    
    internal class ScopeLifetime: IObjectResolver
    {
        private readonly IScopeService scopeService;
        private readonly IObjectResolver objectResolver;
        private readonly Guid key = Guid.NewGuid();

        public ScopeLifetime(IScopeService scopeService, IObjectResolver objectResolver)
        {
            this.scopeService = scopeService;
            this.objectResolver = objectResolver;
        }

        public InstanceLifetime Lifetime => InstanceLifetime.Scoped;

        public object Get()
        {
            return this.scopeService.Current?.Get(key, () => this.objectResolver.Get());
        }

        public void Build()
        {
            this.objectResolver.Build();
        }
    }
}