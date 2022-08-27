namespace Doser.Implementation.Lifetime;

using System;
using System.Collections.Concurrent;

internal class Scope : IScope
{
    private readonly ConcurrentDictionary<Guid, object> instances = new ();
    private readonly ThreadScopeService service;

    public Scope(ThreadScopeService service, IScope parent)
    {
        this.service = service;
        this.Parent = parent;
    }

    public IScope Parent { get; }

    public object Get(Guid key, Func<object> factory)
    {
        return this.instances.GetOrAdd(key, _ => factory());
    }

    public object GetTransparent(Guid key, Func<object> factory)
    {
        return this.Get(key, this.Parent == null 
            ? factory 
            : () => this.Parent.GetTransparent(key, factory));
    }

    public void Dispose()
    {
        this.service.CloseScope(this);
    }
}