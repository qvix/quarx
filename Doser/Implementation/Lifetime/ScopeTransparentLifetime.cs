namespace Doser.Implementation.Lifetime;

using System;
    
internal sealed class ScopeTransparentLifetime : IObjectResolver
{
    private readonly IScopeService scopeService;
    private readonly IObjectResolver objectResolver;
    private readonly Guid key = Guid.NewGuid(); 

    public ScopeTransparentLifetime(IScopeService scopeService, IObjectResolver objectResolver)
    {
        this.scopeService = scopeService;
        this.objectResolver = objectResolver;
    }

    public InstanceLifetime Lifetime => InstanceLifetime.ScopeTransparent;

    public object? Resolve()
    {
        return this.scopeService.Current?.GetTransparent(key, this.objectResolver.Resolve);
    }

    public IObjectResolver Build()
    {
        this.objectResolver.Build();
        return this;
    }
}